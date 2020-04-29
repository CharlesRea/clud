using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;

namespace Clud.Build
{
    public class Build : NukeBuild
    {
        public static int Main() => Execute<Build>(x => x.Compile);

        [Parameter] private readonly string SqlConnectionString = "Host=postgres.clud;Port=30432;Database=clud;Username=clud;Password=supersecret";

        private readonly string Configuration = "Release";

        [Solution("Clud.sln")] private readonly Solution Solution;

        private AbsolutePath SourceDirectory => RootDirectory / "src";
        private AbsolutePath MigrationsDirectory => SourceDirectory / "Migrations";

        private AbsolutePath MigrationsDllFile =>
            MigrationsDirectory / $"bin/{Configuration}/netcoreapp3.1/Migrations.dll";

        private AbsolutePath OutputDirectory => RootDirectory / "output";

        private Target Clean => _ => _
            .Executes(() =>
            {
                DotNetTasks.DotNetClean(s => s
                    .SetConfiguration(Configuration)
                    .SetProject(Solution));
                FileSystemTasks.EnsureCleanDirectory(OutputDirectory);
            });

        private Target Restore => _ => _
            .Executes(() =>
            {
                DotNetTasks.DotNetRestore(s => s.SetProjectFile(Solution));
            })
            .After(Clean);

        private Target Compile => _ => _
            .DependsOn(Restore)
            .Executes(() =>
            {
                DotNetTasks.DotNetBuild(s => s
                    .SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .EnableTreatWarningsAsErrors()
                    .EnableNoRestore());
            });

        private Target MigrateDatabase => _ => _
            .DependsOn(Compile)
            .Requires(() => SqlConnectionString)
            .Executes(() =>
            {
                DotNetTasks.DotNet(MigrationsDllFile + $" --connectionString \"{SqlConnectionString}\"");
            });

        private Target RebuildDatabase => _ => _
            .DependsOn(Compile)
            .Requires(() => SqlConnectionString)
            .Executes(() =>
            {
                DotNetTasks.DotNet(MigrationsDllFile +
                                   $" --connectionString \"{SqlConnectionString}\" " +
                                   $"--recreateDatabase true"
                );
            });
    }
}
