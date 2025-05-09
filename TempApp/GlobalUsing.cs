// GlobalUsings.cs
// Rust numeric primitives → C# type aliases

global using f32   = System.Single;
global using f64   = System.Double;
global using i16   = System.Int16;
global using i32   = System.Int32;
global using i64   = System.Int64;
global using i8    = System.SByte;
global using u16   = System.UInt16;
global using u32   = System.UInt32;
global using u64   = System.UInt64;
global using u8    = System.Byte;

// If you're targeting .NET 7 or later and want 128‑bit aliases:
#if NET7_0_OR_GREATER
global using i128 = System.Int128;
global using u128 = System.UInt128;
#endif