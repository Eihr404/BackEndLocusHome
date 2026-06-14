import 'package:flutter/foundation.dart';
import 'package:graphql_flutter/graphql_flutter.dart';

class GraphQLClientProvider {
  GraphQLClientProvider._();

  static final GraphQLClientProvider instance = GraphQLClientProvider._();

  ValueNotifier<GraphQLClient> get client => _client;

  final ValueNotifier<GraphQLClient> _client = ValueNotifier<GraphQLClient>(
    GraphQLClient(
      link: HttpLink(_resolveEndpoint()),
      cache: GraphQLCache(store: HiveStore()),
      queryRequestTimeout: const Duration(seconds: 20),
    ),
  );

  static String _resolveEndpoint() {
    const configured = String.fromEnvironment('GRAPHQL_ENDPOINT', defaultValue: '');
    if (configured.isNotEmpty) {
      return configured;
    }

    if (!kIsWeb && defaultTargetPlatform == TargetPlatform.android) {
      return 'http://10.0.2.2:5095/graphql';
    }

    return 'http://localhost:5095/graphql';
  }
}
