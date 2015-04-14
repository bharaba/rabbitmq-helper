﻿using Thycotic.InstallerGenerator.Core.MSI.WiX;

namespace Thycotic.InstallerGenerator.Steps.Services
{
    public abstract class BaseServiceWiXMsiGeneratorSteps : WiXMsiGeneratorSteps
    {
        protected static string GetArtifactFileName(string artifactName, string version)
        {
            return string.Format("{0}.{1}.msi", artifactName, version);
        }

        protected BaseServiceWiXMsiGeneratorSteps(string recipePath, string sourcePath, string artifactName, string version)

        {
            RecipePath = recipePath;
            SourcePath = sourcePath;
            ArtifactName = artifactName;

            Substeps = new WiXMsiGeneratorSubsteps
            {

                Heat = string.Format(@"
dir {0}
-nologo
-o output\Autogenerated.wxs 
-ag 
-sfrags 
-suid 
-cg main_component_group 
-t add_service_install.xsl 
-sreg 
-scom 
-srd 
-template fragment 
-dr INSTALLLOCATION", sourcePath),
                Candle = string.Format(@"
-nologo 
-ext WixUtilExtension 
-dInstallerVersion={0} 
-out output\
output\AutoGenerated.wxs Product.wxs", version),
                Light = string.Format(@"
-nologo
-b {0}
-ext WixUIExtension 
-ext WixUtilExtension 
-out {1}
output\AutoGenerated.wixobj output\Product.wixobj", sourcePath, artifactName)
            };
        }
    }
}