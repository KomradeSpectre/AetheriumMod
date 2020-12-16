// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Type Safety", "UNT0016:Unsafe way to get the method name",
                           Justification = "No other way to invoke the method without time.",
                           Scope = "member", Target = "~M:Aetherium.Items.VoidheartDeath.FixedUpdate")]