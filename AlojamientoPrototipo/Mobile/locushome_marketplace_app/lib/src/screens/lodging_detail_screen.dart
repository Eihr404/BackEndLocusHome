import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../core/api/graphql_client_provider.dart';
import '../core/api/marketplace_api.dart';
import '../core/models.dart';
import '../core/session.dart';

class LodgingDetailScreen extends StatefulWidget {
  const LodgingDetailScreen({
    super.key,
    required this.alojamientoId,
    required this.sessionController,
    this.initialCheckIn,
    this.initialCheckOut,
    this.initialAdults = 2,
    this.initialChildren = 0,
    this.initialPets = false,
  });

  final int alojamientoId;
  final SessionController sessionController;
  final DateTime? initialCheckIn;
  final DateTime? initialCheckOut;
  final int initialAdults;
  final int initialChildren;
  final bool initialPets;

  @override
  State<LodgingDetailScreen> createState() => _LodgingDetailScreenState();
}

class _LodgingDetailScreenState extends State<LodgingDetailScreen> {
  late final MarketplaceApi _api;
  late Future<LodgingDetail?> _detailFuture;
  final NumberFormat _currency = NumberFormat.currency(symbol: '\$', decimalDigits: 2);
  late DateTime _selectedMonth;

  @override
  void initState() {
    super.initState();
    _api = MarketplaceApi(GraphQLClientProvider.instance.client.value);
    final initialMonth = widget.initialCheckIn ?? DateTime.now();
    _selectedMonth = DateTime(initialMonth.year, initialMonth.month);
    _detailFuture = _loadDetail();
  }

  Future<LodgingDetail?> _loadDetail() {
    return _api.getMarketplaceAlojamientoDetalleByMonth(
      alojamientoId: widget.alojamientoId,
      mes: _selectedMonth.month,
      anio: _selectedMonth.year,
    );
  }

  Future<void> _changeMonth(int delta) async {
    setState(() {
      _selectedMonth = DateTime(_selectedMonth.year, _selectedMonth.month + delta);
      _detailFuture = _loadDetail();
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Detalle')),
      body: FutureBuilder<LodgingDetail?>(
        future: _detailFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return Center(child: Padding(
              padding: const EdgeInsets.all(24),
              child: Text(snapshot.error.toString()),
            ));
          }

          final detail = snapshot.data;
          if (detail == null) {
            return const Center(child: Text('No se encontro el alojamiento.'));
          }

          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              if (detail.fotos.isNotEmpty)
                SizedBox(
                  height: 220,
                  child: PageView.builder(
                    itemCount: detail.fotos.length,
                    itemBuilder: (context, index) => ClipRRect(
                      borderRadius: BorderRadius.circular(28),
                      child: Image.network(
                        detail.fotos[index].url,
                        fit: BoxFit.cover,
                        errorBuilder: (_, __, ___) => Container(
                          color: const Color(0xFFDCE8E6),
                          child: const Icon(Icons.broken_image_outlined, size: 42),
                        ),
                      ),
                    ),
                  ),
                ),
              const SizedBox(height: 20),
              Text(
                detail.alojamiento.nombre,
                style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
              ),
              const SizedBox(height: 8),
              Text(detail.alojamiento.direccion),
              const SizedBox(height: 16),
              Text(detail.alojamiento.descripcion ?? 'Sin descripcion.'),
              const SizedBox(height: 20),
              Row(
                children: [
                  IconButton(
                    onPressed: () => _changeMonth(-1),
                    icon: const Icon(Icons.chevron_left),
                  ),
                  Expanded(
                    child: Text(
                      DateFormat('MMMM yyyy').format(_selectedMonth),
                      textAlign: TextAlign.center,
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.w700,
                          ),
                    ),
                  ),
                  IconButton(
                    onPressed: () => _changeMonth(1),
                    icon: const Icon(Icons.chevron_right),
                  ),
                ],
              ),
              const SizedBox(height: 8),
              Text(
                widget.initialCheckIn != null && widget.initialCheckOut != null
                    ? 'Estas viendo disponibilidad para la busqueda ${DateFormat('dd/MM').format(widget.initialCheckIn!)} - ${DateFormat('dd/MM').format(widget.initialCheckOut!)}.'
                    : 'Primero revisa el calendario del mes y luego reserva solo en fechas disponibles.',
                style: Theme.of(context).textTheme.bodyMedium,
              ),
              const SizedBox(height: 20),
              Text(
                'Habitaciones disponibles',
                style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w700),
              ),
              const SizedBox(height: 12),
              ...detail.habitaciones.map(
                (room) => Card(
                  child: Padding(
                    padding: const EdgeInsets.all(18),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          room.nombre,
                          style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                fontWeight: FontWeight.w700,
                              ),
                        ),
                        const SizedBox(height: 8),
                        Text(room.descripcion ?? 'Sin descripcion.'),
                        const SizedBox(height: 12),
                        Wrap(
                          spacing: 8,
                          runSpacing: 8,
                          children: [
                            Chip(label: Text('Adultos ${room.capacidadAdultos}')),
                            Chip(label: Text('Ninos ${room.capacidadNinos}')),
                            Chip(label: Text('Banos ${room.numBanos}')),
                            Chip(label: Text('Dormitorios ${room.numDormitorios}')),
                          ],
                        ),
                        const SizedBox(height: 12),
                        _AvailabilityPreview(
                          month: _selectedMonth,
                          room: room,
                        ),
                        const SizedBox(height: 12),
                        Row(
                          children: [
                            Text(
                              _currency.format(room.precioNoche),
                              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                    fontWeight: FontWeight.w700,
                                  ),
                            ),
                            const SizedBox(width: 8),
                            const Text('por noche'),
                            const Spacer(),
                            FilledButton(
                              onPressed: () => _reserveRoom(detail, room),
                              child: const Text('Reservar'),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }

  Future<void> _reserveRoom(LodgingDetail detail, Room room) async {
    final session = widget.sessionController.session!;
    if (session.clienteId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Tu usuario no tiene cliente asociado para reservar.')),
      );
      return;
    }

    final blockedDates = (await _api.getRoomAvailability(
      habitacionId: room.habitacionId,
      mes: _selectedMonth.month,
      anio: _selectedMonth.year,
    ))
        .map((item) => DateTime(item.fecha.year, item.fecha.month, item.fecha.day))
        .toSet();
    if (!mounted) return;

    final firstAllowedDate = _firstAllowedDateForMonth(_selectedMonth, blockedDates);
    if (firstAllowedDate == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('No hay fechas disponibles en el mes seleccionado para esta habitacion.')),
      );
      return;
    }

    final lastDateInMonth = DateTime(_selectedMonth.year, _selectedMonth.month + 1, 0);
    final preferredCheckIn = _preferredCheckInDate(firstAllowedDate, lastDateInMonth, blockedDates);

    final checkIn = await showDatePicker(
      context: context,
      firstDate: firstAllowedDate,
      initialDate: preferredCheckIn,
      lastDate: lastDateInMonth,
      selectableDayPredicate: (day) => _isAvailableDay(day, blockedDates),
    );
    if (checkIn == null || !mounted) return;

    final preferredCheckOut = _preferredCheckOutDate(checkIn, lastDateInMonth, blockedDates);
    final checkOut = await showDatePicker(
      context: context,
      firstDate: checkIn.add(const Duration(days: 1)),
      initialDate: preferredCheckOut,
      lastDate: lastDateInMonth,
      selectableDayPredicate: (day) => _isAvailableRange(checkIn, day, blockedDates),
    );
    if (checkOut == null || !mounted) return;

    final reservationData = await _showReservationDataSheet(session, detail, room);
    if (reservationData == null || !mounted) {
      return;
    }

    final paymentMethods = await _api.getMarketplaceMetodosPago();
    if (!mounted) return;
    if (paymentMethods.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('No hay metodos de pago disponibles.')),
      );
      return;
    }

    final selectedMethod = await showModalBottomSheet<PaymentMethod>(
      context: context,
      builder: (context) => ListView(
        shrinkWrap: true,
        children: paymentMethods
            .map(
              (method) => ListTile(
                title: Text(method.tipo),
                onTap: () => Navigator.of(context).pop(method),
              ),
            )
            .toList(),
      ),
    );

    if (selectedMethod == null || !mounted) return;

    final nights = checkOut.difference(checkIn).inDays;
    final total = room.precioNoche * nights;
    final receiptDraft = _ReservationReceiptDraft(
      alojamientoNombre: detail.alojamiento.nombre,
      habitacionNombre: room.nombre,
      fechaCheckIn: checkIn,
      fechaCheckOut: checkOut,
      adultos: reservationData.adultos,
      ninos: reservationData.ninos,
      llevaMascotas: reservationData.llevaMascotas,
      responsable: reservationData.nombreCompleto,
      documento: reservationData.documento,
      telefono: reservationData.telefono,
      correo: reservationData.correo,
      observaciones: reservationData.observaciones,
      metodoPago: selectedMethod.tipo,
      precioPorNoche: room.precioNoche,
      numNoches: nights,
      total: total,
    );

    final confirmed = await _showReservationReceiptPreview(receiptDraft);
    if (confirmed != true || !mounted) return;

    final reservationKey = 'reservation-${DateTime.now().microsecondsSinceEpoch}-${session.usuarioId}';
    final invoiceKey = 'invoice-${DateTime.now().microsecondsSinceEpoch}-${session.usuarioId}';

    try {
      final reservation = await _api.createReservation(
        clienteId: session.clienteId!,
        alojamientoId: detail.alojamiento.alojamientoId,
        fechaCheckIn: checkIn,
        fechaCheckOut: checkOut,
        numAdultos: reservationData.adultos,
        numNinos: reservationData.ninos,
        llevaMascotas: reservationData.llevaMascotas,
        habitacionId: room.habitacionId,
        precioPorNoche: room.precioNoche,
        numNoches: nights,
        idempotencyKey: reservationKey,
      );

      final invoice = await _api.createInvoice(
        reservaId: reservation.reservaId,
        metodoPagoId: selectedMethod.metodoPagoId,
        monto: total,
        descripcion: 'Reserva ${detail.alojamiento.nombre}',
        cantidad: nights,
        precioUnitario: room.precioNoche,
        idempotencyKey: invoiceKey,
      );

      if (!mounted) return;
      await showDialog<void>(
        context: context,
        builder: (context) => AlertDialog(
          title: const Text('Recibo de reserva'),
          content: SingleChildScrollView(
            child: _ReceiptSummary(
              draft: receiptDraft,
              codigoReserva: reservation.codigoReserva,
              estadoReserva: reservation.estado,
              facturaId: invoice.facturaId,
              estadoFactura: invoice.estado,
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('Cerrar'),
            ),
          ],
        ),
      );
    } catch (error) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error.toString().replaceFirst('Exception: ', ''))),
      );
    }
  }

  Future<_ReservationData?> _showReservationDataSheet(
    LoginSession session,
    LodgingDetail detail,
    Room room,
  ) {
    final nameController = TextEditingController(text: session.nombreCompleto);
    final documentController = TextEditingController();
    final phoneController = TextEditingController();
    final emailController = TextEditingController();
    final notesController = TextEditingController();
    var adultos = widget.initialAdults.clamp(1, room.capacidadAdultos);
    var ninos = widget.initialChildren.clamp(0, room.capacidadNinos);
    var llevaMascotas = detail.alojamiento.admiteMascotas && widget.initialPets;

    return showModalBottomSheet<_ReservationData>(
      context: context,
      isScrollControlled: true,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setModalState) {
            return SafeArea(
              child: Padding(
                padding: EdgeInsets.only(
                  left: 20,
                  right: 20,
                  top: 20,
                  bottom: MediaQuery.of(context).viewInsets.bottom + 20,
                ),
                child: SingleChildScrollView(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Datos de la reserva',
                        style: Theme.of(context).textTheme.titleLarge?.copyWith(
                              fontWeight: FontWeight.w700,
                            ),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        'La habitacion permite hasta ${room.capacidadAdultos} adultos y ${room.capacidadNinos} ninos.',
                        style: Theme.of(context).textTheme.bodyMedium,
                      ),
                      const SizedBox(height: 16),
                      DropdownButtonFormField<int>(
                        initialValue: adultos,
                        decoration: const InputDecoration(labelText: 'Adultos'),
                        items: List.generate(
                          room.capacidadAdultos,
                          (index) => DropdownMenuItem(
                            value: index + 1,
                            child: Text('${index + 1}'),
                          ),
                        ),
                        onChanged: (value) {
                          if (value != null) {
                            setModalState(() {
                              adultos = value;
                            });
                          }
                        },
                      ),
                      const SizedBox(height: 12),
                      DropdownButtonFormField<int>(
                        initialValue: ninos,
                        decoration: const InputDecoration(labelText: 'Ninos'),
                        items: List.generate(
                          room.capacidadNinos + 1,
                          (index) => DropdownMenuItem(
                            value: index,
                            child: Text('$index'),
                          ),
                        ),
                        onChanged: (value) {
                          if (value != null) {
                            setModalState(() {
                              ninos = value;
                            });
                          }
                        },
                      ),
                      const SizedBox(height: 12),
                      SwitchListTile(
                        contentPadding: EdgeInsets.zero,
                        title: const Text('Llevo mascotas'),
                        subtitle: Text(
                          detail.alojamiento.admiteMascotas
                              ? 'Este alojamiento admite mascotas.'
                              : 'Este alojamiento no admite mascotas.',
                        ),
                        value: llevaMascotas,
                        onChanged: detail.alojamiento.admiteMascotas
                            ? (value) {
                                setModalState(() {
                                  llevaMascotas = value;
                                });
                              }
                            : null,
                      ),
                      const SizedBox(height: 12),
                      TextField(
                        controller: nameController,
                        decoration: const InputDecoration(
                          labelText: 'Nombre del responsable',
                        ),
                      ),
                      const SizedBox(height: 12),
                      TextField(
                        controller: documentController,
                        keyboardType: TextInputType.number,
                        decoration: const InputDecoration(
                          labelText: 'Cedula',
                          helperText: 'Debe contener exactamente 10 digitos.',
                        ),
                      ),
                      const SizedBox(height: 12),
                      TextField(
                        controller: phoneController,
                        keyboardType: TextInputType.phone,
                        decoration: const InputDecoration(
                          labelText: 'Telefono',
                          helperText: 'Debe contener exactamente 10 digitos.',
                        ),
                      ),
                      const SizedBox(height: 12),
                      TextField(
                        controller: emailController,
                        keyboardType: TextInputType.emailAddress,
                        decoration: const InputDecoration(
                          labelText: 'Correo',
                        ),
                      ),
                      const SizedBox(height: 12),
                      TextField(
                        controller: notesController,
                        minLines: 2,
                        maxLines: 4,
                        decoration: const InputDecoration(
                          labelText: 'Observaciones',
                        ),
                      ),
                      const SizedBox(height: 16),
                      SizedBox(
                        width: double.infinity,
                        child: FilledButton(
                          onPressed: () {
                            final nombre = nameController.text.trim();
                            final documento = documentController.text.trim();
                            final telefono = phoneController.text.trim();
                            final correo = emailController.text.trim();

                            if (nombre.isEmpty) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(content: Text('Ingresa el nombre del responsable.')),
                              );
                              return;
                            }

                            if (!_isTenDigitNumber(documento)) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(content: Text('La cedula debe tener exactamente 10 digitos.')),
                              );
                              return;
                            }

                            if (!_isTenDigitNumber(telefono)) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(content: Text('El telefono debe tener exactamente 10 digitos.')),
                              );
                              return;
                            }

                            if (!_isValidEmail(correo)) {
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(content: Text('Ingresa un correo valido.')),
                              );
                              return;
                            }

                            Navigator.of(context).pop(
                              _ReservationData(
                                adultos: adultos,
                                ninos: ninos,
                                llevaMascotas: llevaMascotas,
                                nombreCompleto: nombre,
                                documento: documento,
                                telefono: telefono,
                                correo: correo,
                                observaciones: notesController.text.trim(),
                              ),
                            );
                          },
                          child: const Text('Continuar al pago'),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            );
          },
        );
      },
    ).whenComplete(() {
      nameController.dispose();
      documentController.dispose();
      phoneController.dispose();
      emailController.dispose();
      notesController.dispose();
    });
  }

  Future<bool?> _showReservationReceiptPreview(_ReservationReceiptDraft draft) {
    return showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Confirmar reserva'),
        content: SingleChildScrollView(
          child: _ReceiptSummary(
            draft: draft,
            codigoReserva: null,
            estadoReserva: 'Pendiente',
            facturaId: null,
            estadoFactura: 'Pendiente de pago',
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: const Text('Volver'),
          ),
          FilledButton(
            onPressed: () => Navigator.of(context).pop(true),
            child: const Text('Confirmar'),
          ),
        ],
      ),
    );
  }

  DateTime _preferredCheckInDate(
    DateTime firstAllowedDate,
    DateTime lastDateInMonth,
    Set<DateTime> blockedDates,
  ) {
    final desired = widget.initialCheckIn;
    if (desired == null) {
      return firstAllowedDate;
    }

    final normalized = DateTime(desired.year, desired.month, desired.day);
    if (normalized.month != _selectedMonth.month ||
        normalized.year != _selectedMonth.year ||
        normalized.isBefore(firstAllowedDate) ||
        normalized.isAfter(lastDateInMonth) ||
        !_isAvailableDay(normalized, blockedDates)) {
      return firstAllowedDate;
    }

    return normalized;
  }

  DateTime _preferredCheckOutDate(
    DateTime checkIn,
    DateTime lastDateInMonth,
    Set<DateTime> blockedDates,
  ) {
    final desired = widget.initialCheckOut;
    if (desired != null) {
      final normalized = DateTime(desired.year, desired.month, desired.day);
      if (normalized.isAfter(checkIn) &&
          normalized.month == _selectedMonth.month &&
          normalized.year == _selectedMonth.year &&
          !normalized.isAfter(lastDateInMonth) &&
          _isAvailableRange(checkIn, normalized, blockedDates)) {
        return normalized;
      }
    }

    var candidate = checkIn.add(const Duration(days: 1));
    while (!candidate.isAfter(lastDateInMonth)) {
      if (_isAvailableRange(checkIn, candidate, blockedDates)) {
        return candidate;
      }
      candidate = candidate.add(const Duration(days: 1));
    }

    return checkIn.add(const Duration(days: 1));
  }

  DateTime? _firstAllowedDateForMonth(DateTime month, Set<DateTime> blockedDates) {
    final now = DateTime.now();
    final firstDay = DateTime(month.year, month.month, 1);
    final start = firstDay.isBefore(DateTime(now.year, now.month, now.day))
        ? DateTime(now.year, now.month, now.day)
        : firstDay;
    final end = DateTime(month.year, month.month + 1, 0);

    for (var day = start; !day.isAfter(end); day = day.add(const Duration(days: 1))) {
      if (_isAvailableDay(day, blockedDates)) {
        return day;
      }
    }

    return null;
  }

  bool _isAvailableDay(DateTime day, Set<DateTime> blockedDates) {
    final normalized = DateTime(day.year, day.month, day.day);
    final today = DateTime.now();
    final min = DateTime(today.year, today.month, today.day);
    return !normalized.isBefore(min) && !blockedDates.contains(normalized);
  }

  bool _isAvailableRange(DateTime checkIn, DateTime checkOut, Set<DateTime> blockedDates) {
    final normalized = DateTime(checkOut.year, checkOut.month, checkOut.day);
    if (!normalized.isAfter(DateTime(checkIn.year, checkIn.month, checkIn.day))) {
      return false;
    }

    for (var day = DateTime(checkIn.year, checkIn.month, checkIn.day);
        !day.isAfter(normalized);
        day = day.add(const Duration(days: 1))) {
      if (blockedDates.contains(DateTime(day.year, day.month, day.day))) {
        return false;
      }
    }

    return true;
  }

  bool _isTenDigitNumber(String value) => RegExp(r'^\d{10}$').hasMatch(value);

  bool _isValidEmail(String value) =>
      RegExp(r'^[^@\s]+@[^@\s]+\.[^@\s]+$').hasMatch(value);
}

class _ReservationData {
  const _ReservationData({
    required this.adultos,
    required this.ninos,
    required this.llevaMascotas,
    required this.nombreCompleto,
    required this.documento,
    required this.telefono,
    required this.correo,
    required this.observaciones,
  });

  final int adultos;
  final int ninos;
  final bool llevaMascotas;
  final String nombreCompleto;
  final String documento;
  final String telefono;
  final String correo;
  final String observaciones;
}

class _ReservationReceiptDraft {
  const _ReservationReceiptDraft({
    required this.alojamientoNombre,
    required this.habitacionNombre,
    required this.fechaCheckIn,
    required this.fechaCheckOut,
    required this.adultos,
    required this.ninos,
    required this.llevaMascotas,
    required this.responsable,
    required this.documento,
    required this.telefono,
    required this.correo,
    required this.observaciones,
    required this.metodoPago,
    required this.precioPorNoche,
    required this.numNoches,
    required this.total,
  });

  final String alojamientoNombre;
  final String habitacionNombre;
  final DateTime fechaCheckIn;
  final DateTime fechaCheckOut;
  final int adultos;
  final int ninos;
  final bool llevaMascotas;
  final String responsable;
  final String documento;
  final String telefono;
  final String correo;
  final String observaciones;
  final String metodoPago;
  final double precioPorNoche;
  final int numNoches;
  final double total;
}

class _ReceiptSummary extends StatelessWidget {
  const _ReceiptSummary({
    required this.draft,
    required this.codigoReserva,
    required this.estadoReserva,
    required this.facturaId,
    required this.estadoFactura,
  });

  final _ReservationReceiptDraft draft;
  final String? codigoReserva;
  final String estadoReserva;
  final int? facturaId;
  final String estadoFactura;

  @override
  Widget build(BuildContext context) {
    final currency = NumberFormat.currency(symbol: '\$', decimalDigits: 2);

    Widget line(String label, String value) {
      return Padding(
        padding: const EdgeInsets.only(bottom: 8),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            SizedBox(
              width: 110,
              child: Text(
                label,
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
              ),
            ),
            Expanded(
              child: Text(value.isEmpty ? '-' : value),
            ),
          ],
        ),
      );
    }

    return Column(
      mainAxisSize: MainAxisSize.min,
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        line('Alojamiento', draft.alojamientoNombre),
        line('Habitacion', draft.habitacionNombre),
        line('Entrada', DateFormat('dd/MM/yyyy').format(draft.fechaCheckIn)),
        line('Salida', DateFormat('dd/MM/yyyy').format(draft.fechaCheckOut)),
        line('Huespedes', '${draft.adultos} adultos, ${draft.ninos} ninos'),
        line('Mascotas', draft.llevaMascotas ? 'Si' : 'No'),
        line('Responsable', draft.responsable),
        line('Documento', draft.documento),
        line('Telefono', draft.telefono),
        line('Correo', draft.correo),
        line('Pago', draft.metodoPago),
        line('Noches', '${draft.numNoches}'),
        line('Precio/noche', currency.format(draft.precioPorNoche)),
        line('Total', currency.format(draft.total)),
        if (draft.observaciones.isNotEmpty) line('Observaciones', draft.observaciones),
        if (codigoReserva != null) line('Codigo', codigoReserva!),
        line('Estado reserva', estadoReserva),
        if (facturaId != null) line('Factura', '$facturaId'),
        line('Estado factura', estadoFactura),
      ],
    );
  }
}

class _AvailabilityPreview extends StatelessWidget {
  const _AvailabilityPreview({
    required this.month,
    required this.room,
  });

  final DateTime month;
  final Room room;

  @override
  Widget build(BuildContext context) {
    final lastDay = DateTime(month.year, month.month + 1, 0).day;
    final blocked = room.disponibilidadMensual
        .map((item) => item.fecha.day)
        .toSet();
    final today = DateTime.now();
    final availableDays = <int>[];

    for (var day = 1; day <= lastDay; day++) {
      final current = DateTime(month.year, month.month, day);
      final isPast = current.isBefore(DateTime(today.year, today.month, today.day));
      if (!isPast && !blocked.contains(day)) {
        availableDays.add(day);
      }
    }

    if (availableDays.isEmpty) {
      return Text(
        'No hay dias disponibles en este mes.',
        style: Theme.of(context).textTheme.bodyMedium?.copyWith(
              color: Colors.red.shade700,
            ),
      );
    }

    final preview = availableDays.take(10).join(', ');
    final suffix = availableDays.length > 10 ? '...' : '';

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Dias disponibles: $preview$suffix',
          style: Theme.of(context).textTheme.bodyMedium,
        ),
        const SizedBox(height: 4),
        Text(
          'Los dias bloqueados u ocupados no se podran seleccionar al reservar.',
          style: Theme.of(context).textTheme.bodySmall,
        ),
      ],
    );
  }
}
