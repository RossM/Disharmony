; Unreleased analyzer changes

### New Rules

Rule ID        | Category    | Severity | Notes
---------------|-------------|----------|------
DISHARMONY0001 | Correctness | Error    | Patch methods cannot contain generic parameters
DISHARMONY0002 | Correctness | Error    | Patch methods must be static
DISHARMONY0003 | Correctness | Error    | Prefixes must return bool or void
DISHARMONY0004 | Correctness | Error    | Postfixes must return void
DISHARMONY0005 | Correctness | Error    | AlwaysRun prefixes must return void
DISHARMONY0006 | Correctness | Warning  | Patch methods require a discoverable containing class
DISHARMONY0007 | Correctness | Warning  | Patch methods require a target attribute
DISHARMONY0008 | Correctness | Warning  | Direct Disharmony method attributes require a patch type
DISHARMONY0009 | Correctness | Error    | Multiple patch type attributes
DISHARMONY0010 | Correctness | Error    | Multiple inner target attributes
DISHARMONY0011 | Correctness | Error    | Missing selector type and qualified name
DISHARMONY0012 | Correctness | Warning  | Null inner constant
DISHARMONY0014 | Correctness | Warning  | Duplicate patch markers or categories, including mixed Disharmony/Harmony attributes
DISHARMONY0015 | Correctness | Error    | Missing non-constructor member name
DISHARMONY0016 | Correctness | Error    | Multiple parameter binding attributes
DISHARMONY0017 | Correctness | Error    | Inner-only binding on an ordinary patch
DISHARMONY0018 | Correctness | Error    | Result binding in an AlwaysRun prefix
DISHARMONY0019 | Correctness | Error    | Exception binding outside an AlwaysRun postfix
DISHARMONY0020 | Correctness | Error    | Method binding requires a concrete delegate passed by value
DISHARMONY0021 | Correctness | Error    | Incompatible binding type for exceptions or inner constants
DISHARMONY0022 | Correctness | Error    | Conflicting types for a shared state key
DISHARMONY0024 | Correctness | Error    | Unavailable instance, argument, or field on an inner constant
DISHARMONY0025 | Correctness | Warning  | Void prefix binds the result
DISHARMONY0027 | Correctness | Error    | Unknown special parameter name
DISHARMONY0028 | Correctness | Warning  | Multiple parameters bind the same value in a patch
DISHARMONY0029 | Correctness | Warning  | State key has no ref or out parameter in its patch class
DISHARMONY0030 | Correctness | Warning  | State key is only bound through out parameters in its patch class
DISHARMONY0031 | Correctness | Warning  | Patch writes to a parameter passed by value
DISHARMONY0032 | Correctness | Warning  | AlwaysRun patch explicitly throws
DISHARMONY0034 | Style       | Warning  | Prefix result is ref
DISHARMONY0035 | Correctness | Warning  | Postfix argument binding is out
DISHARMONY0036 | Correctness | Error | Arguments and MemberInfo bindings must be passed by value
DISHARMONY0037 | Correctness | Error | Inner constant cannot supply member metadata or a base method
DISHARMONY0038 | Correctness | Error | MemberInfo parameter cannot accept any supported member metadata type
