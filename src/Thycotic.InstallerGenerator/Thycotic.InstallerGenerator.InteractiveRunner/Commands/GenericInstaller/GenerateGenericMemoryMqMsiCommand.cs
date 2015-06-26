﻿using Thycotic.CLI.Commands;
using Thycotic.InstallerGenerator.Core;
using Thycotic.InstallerGenerator.Core.WiX;
using Thycotic.InstallerGenerator.Runbooks.Services;
using Thycotic.Logging;

namespace Thycotic.InstallerGenerator.InteractiveRunner.Commands.GenericInstaller
{
    class GenerateGenericMemoryMqMsiCommand : CommandBase, IImmediateCommand
    {
        private readonly ILogWriter _log = Log.Get(typeof(GenerateGenericMemoryMqMsiCommand));

        public override string Area
        {
            get { return CommandAreas.Generic; }
        }

        public override string Description
        {
            get { return "Generates generic MemoryMq Site Connector MSI"; }
        }

        public GenerateGenericMemoryMqMsiCommand()
        {

            Action = parameters =>
            {
                bool is32Bit;
                parameters.TryGetBoolean("Is32Bit", out is32Bit);
                var artifactPath = parameters["ArtifactPath"];
                var binariesSourcePath = parameters["SourcePath.Binaries"];
                var recipesSourcePath = parameters["SourcePath.Recipes"];

                var installerVersion = parameters["Installer.Version"];

                var pfxPath = parameters["Signing.PfxPath"];
                var pfxPassword = parameters["Signing.PfxPassword"];

                var steps = new GenericMemoryMqSiteConnectorServiceWiXMsiGeneratorRunbook
                {
                    Is64Bit = !is32Bit,
                    ArtifactPath = artifactPath,
                    RecipePath = recipesSourcePath,
                    SourcePath = binariesSourcePath,
                    Version = installerVersion,

                    PfxPath = pfxPath,
                    PfxPassword = pfxPassword,

                    HeatPathProvider = applicationPath => WiX.ToolPaths.GetHeatPath(applicationPath),
                    CandlePathProvider = applicationPath => WiX.ToolPaths.GetCandlePath(applicationPath),
                    LightPathProvider = applicationPath => WiX.ToolPaths.GetLightPath(applicationPath),

                    SignToolPathProvider = applicationPath => ToolPaths.GetSignToolPath(applicationPath),
                };
                
                var wrapper = new InstallerGeneratorWrapper();

                var path = wrapper.Generate(new Generator(), steps);

                return 0;

            };
        }
    }
}
