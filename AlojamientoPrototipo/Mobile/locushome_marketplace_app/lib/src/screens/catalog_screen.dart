import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../core/api/graphql_client_provider.dart';
import '../core/api/marketplace_api.dart';
import '../core/models.dart';
import '../core/session.dart';
import 'lodging_detail_screen.dart';
import 'reservations_screen.dart';

class CatalogScreen extends StatefulWidget {
  const CatalogScreen({
    super.key,
    required this.sessionController,
  });

  final SessionController sessionController;

  @override
  State<CatalogScreen> createState() => _CatalogScreenState();
}

class _CatalogScreenState extends State<CatalogScreen> {
  late final MarketplaceApi _api;
  late Future<List<LodgingCard>> _catalogFuture;
  final NumberFormat _currency = NumberFormat.currency(symbol: '\$', decimalDigits: 2);
  final TextEditingController _ubicacionController = TextEditingController();
  DateTime? _fechaCheckIn;
  DateTime? _fechaCheckOut;
  int _numAdultos = 2;
  int _numNinos = 0;
  bool _admiteMascotas = false;
  bool _tienePiscina = false;
  bool _tieneParqueadero = false;
  bool _hasSubmittedSearch = false;

  @override
  void initState() {
    super.initState();
    _api = MarketplaceApi(GraphQLClientProvider.instance.client.value);
    _catalogFuture = _loadCatalog();
  }

  @override
  void dispose() {
    _ubicacionController.dispose();
    super.dispose();
  }

  Future<List<LodgingCard>> _loadCatalog() {
    return _api.getMarketplaceCatalog(
      ubicacion: _ubicacionController.text,
      fechaCheckIn: _fechaCheckIn,
      fechaCheckOut: _fechaCheckOut,
      numAdultos: _numAdultos,
      numNinos: _numNinos,
      admiteMascotas: _admiteMascotas ? true : null,
      tienePiscina: _tienePiscina ? true : null,
      tieneParqueadero: _tieneParqueadero ? true : null,
    );
  }

  Future<void> _refresh() async {
    setState(() {
      _catalogFuture = _loadCatalog();
    });
    await _catalogFuture;
  }

  void _search() {
    if (_fechaCheckIn != null && _fechaCheckOut == null) {
      _showMessage('Selecciona la fecha de salida para completar la busqueda.');
      return;
    }

    if (_fechaCheckOut != null && _fechaCheckIn == null) {
      _showMessage('Selecciona la fecha de entrada para completar la busqueda.');
      return;
    }

    if (_fechaCheckIn != null &&
        _fechaCheckOut != null &&
        !_fechaCheckOut!.isAfter(_fechaCheckIn!)) {
      _showMessage('La fecha de salida debe ser posterior a la fecha de entrada.');
      return;
    }

    setState(() {
      _hasSubmittedSearch = true;
      _catalogFuture = _loadCatalog();
    });
  }

  void _clearFilters() {
    setState(() {
      _ubicacionController.clear();
      _fechaCheckIn = null;
      _fechaCheckOut = null;
      _numAdultos = 2;
      _numNinos = 0;
      _admiteMascotas = false;
      _tienePiscina = false;
      _tieneParqueadero = false;
      _hasSubmittedSearch = false;
      _catalogFuture = _loadCatalog();
    });
  }

  Future<void> _pickCheckInDate() async {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);
    final initial = _fechaCheckIn ?? today;
    final picked = await showDatePicker(
      context: context,
      firstDate: today,
      initialDate: initial,
      lastDate: DateTime(today.year + 2),
    );

    if (picked == null) {
      return;
    }

    setState(() {
      _fechaCheckIn = picked;
      if (_fechaCheckOut != null && !_fechaCheckOut!.isAfter(picked)) {
        _fechaCheckOut = null;
      }
    });
  }

  Future<void> _pickCheckOutDate() async {
    if (_fechaCheckIn == null) {
      _showMessage('Primero selecciona la fecha de entrada.');
      return;
    }

    final initial = _fechaCheckOut ?? _fechaCheckIn!.add(const Duration(days: 1));
    final picked = await showDatePicker(
      context: context,
      firstDate: _fechaCheckIn!.add(const Duration(days: 1)),
      initialDate: initial,
      lastDate: DateTime(_fechaCheckIn!.year + 2),
    );

    if (picked == null) {
      return;
    }

    setState(() {
      _fechaCheckOut = picked;
    });
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message)),
    );
  }

  @override
  Widget build(BuildContext context) {
    final session = widget.sessionController.session!;

    return Scaffold(
      appBar: AppBar(
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('LocusHome'),
            Text(
              session.nombreCompleto,
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ],
        ),
        actions: [
          IconButton(
            tooltip: 'Mis reservas',
            onPressed: session.clienteId == null
                ? null
                : () {
                    Navigator.of(context).push(
                      MaterialPageRoute(
                        builder: (_) => ReservationsScreen(
                          clienteId: session.clienteId!,
                        ),
                      ),
                    );
                  },
            icon: const Icon(Icons.receipt_long_outlined),
          ),
          IconButton(
            tooltip: 'Cerrar sesion',
            onPressed: widget.sessionController.signOut,
            icon: const Icon(Icons.logout),
          ),
        ],
      ),
      body: Column(
        children: [
          Expanded(
            child: RefreshIndicator(
              onRefresh: _refresh,
              child: FutureBuilder<List<LodgingCard>>(
                future: _catalogFuture,
                builder: (context, snapshot) {
                  if (snapshot.connectionState == ConnectionState.waiting) {
                    return ListView(
                      padding: const EdgeInsets.all(16),
                      children: const [
                        SizedBox(height: 24),
                        Center(child: CircularProgressIndicator()),
                      ],
                    );
                  }

                  if (snapshot.hasError) {
                    return ListView(
                      padding: const EdgeInsets.all(16),
                      children: [
                        _SearchPanel(
                          ubicacionController: _ubicacionController,
                          fechaCheckIn: _fechaCheckIn,
                          fechaCheckOut: _fechaCheckOut,
                          numAdultos: _numAdultos,
                          numNinos: _numNinos,
                          admiteMascotas: _admiteMascotas,
                          tienePiscina: _tienePiscina,
                          tieneParqueadero: _tieneParqueadero,
                          onPickCheckIn: _pickCheckInDate,
                          onPickCheckOut: _pickCheckOutDate,
                          onAdultosChanged: (value) => setState(() => _numAdultos = value),
                          onNinosChanged: (value) => setState(() => _numNinos = value),
                          onMascotasChanged: (value) => setState(() => _admiteMascotas = value),
                          onPiscinaChanged: (value) => setState(() => _tienePiscina = value),
                          onParqueaderoChanged: (value) => setState(() => _tieneParqueadero = value),
                          onSearch: _search,
                          onClear: _clearFilters,
                        ),
                        const SizedBox(height: 24),
                        Center(
                          child: Padding(
                            padding: const EdgeInsets.all(24),
                            child: Text(snapshot.error.toString()),
                          ),
                        ),
                      ],
                    );
                  }

                  final items = snapshot.data ?? const [];
                  return ListView.separated(
                    padding: const EdgeInsets.all(16),
                    itemCount: items.isEmpty ? 2 : items.length + 1,
                    separatorBuilder: (_, __) => const SizedBox(height: 16),
                    itemBuilder: (context, index) {
                      if (index == 0) {
                        return _SearchPanel(
                          ubicacionController: _ubicacionController,
                          fechaCheckIn: _fechaCheckIn,
                          fechaCheckOut: _fechaCheckOut,
                          numAdultos: _numAdultos,
                          numNinos: _numNinos,
                          admiteMascotas: _admiteMascotas,
                          tienePiscina: _tienePiscina,
                          tieneParqueadero: _tieneParqueadero,
                          onPickCheckIn: _pickCheckInDate,
                          onPickCheckOut: _pickCheckOutDate,
                          onAdultosChanged: (value) => setState(() => _numAdultos = value),
                          onNinosChanged: (value) => setState(() => _numNinos = value),
                          onMascotasChanged: (value) => setState(() => _admiteMascotas = value),
                          onPiscinaChanged: (value) => setState(() => _tienePiscina = value),
                          onParqueaderoChanged: (value) => setState(() => _tieneParqueadero = value),
                          onSearch: _search,
                          onClear: _clearFilters,
                        );
                      }

                      if (items.isEmpty && index == 1) {
                        return Padding(
                          padding: const EdgeInsets.only(top: 56),
                          child: Center(
                            child: Text(
                              _hasSubmittedSearch
                                  ? 'No encontramos alojamientos para esos filtros.'
                                  : 'Ajusta fechas, ubicacion y preferencias para buscar.',
                            ),
                          ),
                        );
                      }

                      final item = items[index - 1];
                      return InkWell(
                        borderRadius: BorderRadius.circular(24),
                        onTap: () {
                          Navigator.of(context).push(
                            MaterialPageRoute(
                              builder: (_) => LodgingDetailScreen(
                                alojamientoId: item.alojamientoId,
                                sessionController: widget.sessionController,
                                initialCheckIn: _fechaCheckIn,
                                initialCheckOut: _fechaCheckOut,
                                initialAdults: _numAdultos,
                                initialChildren: _numNinos,
                                initialPets: _admiteMascotas,
                              ),
                            ),
                          );
                        },
                        child: Card(
                          clipBehavior: Clip.antiAlias,
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              AspectRatio(
                                aspectRatio: 16 / 9,
                                child: item.imagenUrl == null
                                    ? Container(
                                        color: const Color(0xFFDCE8E6),
                                        child: const Center(
                                          child: Icon(Icons.hotel_outlined, size: 48),
                                        ),
                                      )
                                    : Image.network(
                                        item.imagenUrl!,
                                        fit: BoxFit.cover,
                                        errorBuilder: (_, __, ___) => Container(
                                          color: const Color(0xFFDCE8E6),
                                          child: const Center(
                                            child: Icon(Icons.broken_image_outlined, size: 48),
                                          ),
                                        ),
                                      ),
                              ),
                              Padding(
                                padding: const EdgeInsets.all(18),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Wrap(
                                      spacing: 8,
                                      runSpacing: 8,
                                      children: [
                                        Chip(label: Text(item.tipoAlojamiento)),
                                        Chip(label: Text(item.ciudad)),
                                        if (item.admiteMascotas) const Chip(label: Text('Mascotas')),
                                      ],
                                    ),
                                    const SizedBox(height: 12),
                                    Text(
                                      item.nombre,
                                      style: Theme.of(context).textTheme.titleLarge?.copyWith(
                                            fontWeight: FontWeight.w700,
                                          ),
                                    ),
                                    const SizedBox(height: 8),
                                    Text(item.direccion),
                                    const SizedBox(height: 12),
                                    Text(
                                      item.descripcion ?? 'Sin descripcion disponible.',
                                      maxLines: 3,
                                      overflow: TextOverflow.ellipsis,
                                    ),
                                    const SizedBox(height: 16),
                                    Row(
                                      children: [
                                        Text(
                                          _currency.format(item.precioNocheMinimo),
                                          style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                                fontWeight: FontWeight.w700,
                                              ),
                                        ),
                                        const SizedBox(width: 8),
                                        const Text('por noche'),
                                        const Spacer(),
                                        Text(item.disponible ? 'Disponible' : 'No disponible'),
                                      ],
                                    ),
                                  ],
                                ),
                              ),
                            ],
                          ),
                        ),
                      );
                    },
                  );
                },
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _SearchPanel extends StatelessWidget {
  const _SearchPanel({
    required this.ubicacionController,
    required this.fechaCheckIn,
    required this.fechaCheckOut,
    required this.numAdultos,
    required this.numNinos,
    required this.admiteMascotas,
    required this.tienePiscina,
    required this.tieneParqueadero,
    required this.onPickCheckIn,
    required this.onPickCheckOut,
    required this.onAdultosChanged,
    required this.onNinosChanged,
    required this.onMascotasChanged,
    required this.onPiscinaChanged,
    required this.onParqueaderoChanged,
    required this.onSearch,
    required this.onClear,
  });

  final TextEditingController ubicacionController;
  final DateTime? fechaCheckIn;
  final DateTime? fechaCheckOut;
  final int numAdultos;
  final int numNinos;
  final bool admiteMascotas;
  final bool tienePiscina;
  final bool tieneParqueadero;
  final Future<void> Function() onPickCheckIn;
  final Future<void> Function() onPickCheckOut;
  final ValueChanged<int> onAdultosChanged;
  final ValueChanged<int> onNinosChanged;
  final ValueChanged<bool> onMascotasChanged;
  final ValueChanged<bool> onPiscinaChanged;
  final ValueChanged<bool> onParqueaderoChanged;
  final VoidCallback onSearch;
  final VoidCallback onClear;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Busca tu estadia',
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.w700,
                  ),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: ubicacionController,
              textInputAction: TextInputAction.search,
              onSubmitted: (_) => onSearch(),
              decoration: const InputDecoration(
                labelText: 'Ubicacion',
                hintText: 'Ciudad, zona o nombre del alojamiento',
                prefixIcon: Icon(Icons.place_outlined),
              ),
            ),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(
                  child: OutlinedButton.icon(
                    onPressed: onPickCheckIn,
                    icon: const Icon(Icons.calendar_today_outlined),
                    label: Text(
                      fechaCheckIn == null
                          ? 'Entrada'
                          : DateFormat('dd/MM/yyyy').format(fechaCheckIn!),
                    ),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: OutlinedButton.icon(
                    onPressed: onPickCheckOut,
                    icon: const Icon(Icons.event_available_outlined),
                    label: Text(
                      fechaCheckOut == null
                          ? 'Salida'
                          : DateFormat('dd/MM/yyyy').format(fechaCheckOut!),
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(
                  child: DropdownButtonFormField<int>(
                    initialValue: numAdultos,
                    decoration: const InputDecoration(labelText: 'Adultos'),
                    items: List.generate(
                      8,
                      (index) => DropdownMenuItem(
                        value: index + 1,
                        child: Text('${index + 1}'),
                      ),
                    ),
                    onChanged: (value) {
                      if (value != null) {
                        onAdultosChanged(value);
                      }
                    },
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: DropdownButtonFormField<int>(
                    initialValue: numNinos,
                    decoration: const InputDecoration(labelText: 'Ninos'),
                    items: List.generate(
                      7,
                      (index) => DropdownMenuItem(
                        value: index,
                        child: Text('$index'),
                      ),
                    ),
                    onChanged: (value) {
                      if (value != null) {
                        onNinosChanged(value);
                      }
                    },
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: [
                FilterChip(
                  label: const Text('Mascotas'),
                  selected: admiteMascotas,
                  onSelected: onMascotasChanged,
                ),
                FilterChip(
                  label: const Text('Piscina'),
                  selected: tienePiscina,
                  onSelected: onPiscinaChanged,
                ),
                FilterChip(
                  label: const Text('Parqueadero'),
                  selected: tieneParqueadero,
                  onSelected: onParqueaderoChanged,
                ),
              ],
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                  child: FilledButton.icon(
                    onPressed: onSearch,
                    icon: const Icon(Icons.search),
                    label: const Text('Buscar'),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: OutlinedButton(
                    onPressed: onClear,
                    child: const Text('Limpiar'),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
