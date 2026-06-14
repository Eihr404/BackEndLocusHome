import 'package:flutter/material.dart';
import 'package:graphql_flutter/graphql_flutter.dart';

import 'core/api/graphql_client_provider.dart';
import 'core/session.dart';
import 'screens/catalog_screen.dart';
import 'screens/login_screen.dart';

class LocusHomeMarketplaceApp extends StatefulWidget {
  const LocusHomeMarketplaceApp({super.key});

  @override
  State<LocusHomeMarketplaceApp> createState() => _LocusHomeMarketplaceAppState();
}

class _LocusHomeMarketplaceAppState extends State<LocusHomeMarketplaceApp> {
  final SessionController _sessionController = SessionController();

  @override
  Widget build(BuildContext context) {
    return GraphQLProvider(
      client: GraphQLClientProvider.instance.client,
      child: MaterialApp(
        debugShowCheckedModeBanner: false,
        title: 'LocusHome Marketplace',
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(
            seedColor: const Color(0xFF0A6C74),
            brightness: Brightness.light,
          ),
          scaffoldBackgroundColor: const Color(0xFFF4F6F3),
          useMaterial3: true,
          appBarTheme: const AppBarTheme(
            backgroundColor: Colors.transparent,
            foregroundColor: Color(0xFF173231),
            elevation: 0,
          ),
          cardTheme: CardThemeData(
            color: Colors.white,
            elevation: 1,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(24),
            ),
          ),
        ),
        home: AnimatedBuilder(
          animation: _sessionController,
          builder: (context, _) {
            final session = _sessionController.session;
            if (session == null) {
              return LoginScreen(sessionController: _sessionController);
            }

            return CatalogScreen(sessionController: _sessionController);
          },
        ),
      ),
    );
  }
}
