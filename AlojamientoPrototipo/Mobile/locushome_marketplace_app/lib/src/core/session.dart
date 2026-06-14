import 'package:flutter/foundation.dart';

import 'models.dart';

class SessionController extends ChangeNotifier {
  LoginSession? _session;

  LoginSession? get session => _session;

  void signIn(LoginSession session) {
    _session = session;
    notifyListeners();
  }

  void signOut() {
    _session = null;
    notifyListeners();
  }
}
