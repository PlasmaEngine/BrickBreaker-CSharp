InputAction
{
  string %Set{"BrickBreaker"}
  string %Action{"Left"}
  bool %TimeScale{false}
  Slot { string %Key{"keyboard_a"} float %Scale{1} }
  Slot { string %Key{"keyboard_left"} float %Scale{1} }
}

InputAction
{
  string %Set{"BrickBreaker"}
  string %Action{"Right"}
  bool %TimeScale{false}
  Slot { string %Key{"keyboard_d"} float %Scale{1} }
  Slot { string %Key{"keyboard_right"} float %Scale{1} }
}

InputAction
{
  string %Set{"BrickBreaker"}
  string %Action{"Launch"}
  bool %TimeScale{false}
  Slot { string %Key{"keyboard_space"} float %Scale{1} }
  Slot { string %Key{"mouse_button_0"} float %Scale{1} }
}

InputAction
{
  string %Set{"BrickBreaker"}
  string %Action{"Pause"}
  bool %TimeScale{false}
  Slot { string %Key{"keyboard_p"} float %Scale{1} }
  Slot { string %Key{"keyboard_escape"} float %Scale{1} }
}

InputAction
{
  string %Set{"BrickBreaker"}
  string %Action{"Restart"}
  bool %TimeScale{false}
  Slot { string %Key{"keyboard_r"} float %Scale{1} }
}
