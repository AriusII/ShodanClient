using System.Runtime.Versioning;

// v1 is Windows-only (see plan): this lets DPAPI credential storage call
// System.Security.Cryptography.ProtectedData without tripping the CA1416 platform-compatibility
// analyzer under TreatWarningsAsErrors. The target framework itself is left as the OS-agnostic
// "net10.0" (matching the rest of the solution) rather than "net10.0-windows".
[assembly: SupportedOSPlatform("windows")]
