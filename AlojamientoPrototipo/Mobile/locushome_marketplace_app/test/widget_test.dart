import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:locushome_marketplace_app/src/core/session.dart';
import 'package:locushome_marketplace_app/src/screens/login_screen.dart';

void main() {
  testWidgets('renderiza el formulario de acceso del marketplace', (WidgetTester tester) async {
    await tester.pumpWidget(
      MaterialApp(
        home: LoginScreen(
          sessionController: SessionController(),
        ),
      ),
    );

    expect(find.text('Marketplace movil'), findsOneWidget);
    expect(find.text('Entrar'), findsOneWidget);
    expect(find.byType(TextFormField), findsNWidgets(2));
  });
}
