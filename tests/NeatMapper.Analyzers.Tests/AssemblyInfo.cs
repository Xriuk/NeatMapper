using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: SuppressMessage("Style", "IDE0130", Justification = "Project has a single namespace", Scope = "namespace", Target = "~N:NeatMapper")]

[assembly: InternalsVisibleTo("NeatMapper.EntityFrameworkCore")]
[assembly: InternalsVisibleTo("NeatMapper.EntityFrameworkCore.Tests")]
[assembly: InternalsVisibleTo("NeatMapper.Transitive")]
[assembly: InternalsVisibleTo("NeatMapper.Tests")]