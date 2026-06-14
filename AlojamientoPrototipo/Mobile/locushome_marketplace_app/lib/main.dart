import 'package:flutter/material.dart';
import 'package:graphql_flutter/graphql_flutter.dart';

import 'src/app.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await initHiveForFlutter();
  runApp(const LocusHomeMarketplaceApp());
}
