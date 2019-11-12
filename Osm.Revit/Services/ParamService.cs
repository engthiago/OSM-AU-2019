using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit.Services
{
    public class ParamService
    {
        static public string ParamFile => "OSM.Shared.txt";

        static readonly string sourceFile = "OSM.Shared.Source.json";
        static readonly string defGroupName = "OSM";

        // Shared

        static readonly string nameParamName = "Name";
        static readonly string nameParamGUID = "E513F3CE-5784-4936-AB26-CBE1C2212CB3";
        static readonly string nameParamDescription = "The common name for Open Street Map Elements";

        // Building

        static readonly string buildingtypeParamName = "Building Type";
        static readonly string buildingtypeParmGUID = "2F7AE11B-BADA-4BC3-8FBA-BD62175521FA";

        static readonly string streetNumberParamName = "Street Number";
        static readonly string streetNumberParamGUID = "47D313E6-AA4E-43A7-A650-5A9AA29B3D08";

        static readonly string streetNameParamName = "Street Name";
        static readonly string streetNameParamGUID = "0A337537-D2AA-4F03-8899-FDE6CE993592";

        static readonly string postalCode = "Postal Code";
        static readonly string postalCodeGUID = "6E4F7194-A53C-4DD8-85A4-4724650A0025";

        // Street

        static readonly string streetUsageParamName = "Usage";
        static readonly string streetUsageParamGUID = "18C7DEEF-6FDC-4B31-BE2A-F47EC9787D40";
        static readonly string streetUsageDescription = "Usage of the street, refered as 'highway' by OSM";

        static readonly string streetTypeParamName = "Type";
        static readonly string streetTypeParamGUID = "2F314F17-DB21-430C-BD3F-C6B7029C28F9";
        static readonly string streetTypeDescription = "Type of the street, refered as 'tiger:name_type' by OSM";

        static readonly string streetLanesParam = "Lanes";
        static readonly string streetLanesParamGUID = "965B8243-1D52-4E74-BDF7-62E9437615E7";
        static readonly string streetLanesDescription = "Total lanes of the street";

        static readonly string streetMaxSpeedParam = "Max Speed";
        static readonly string streetMaxSpeedParamGUID = "F1195882-5C16-4B35-8D73-35E8F3C05A42";
        static readonly string streetMaxSpeedDescription = "Maximum speed of the street, in Km/h";

        static readonly string streetSurfaceParam = "Surface";
        static readonly string streetSurfaceParamGUID = "7F558884-6311-4186-8CCC-CA4E9989469B";
        static readonly string streetSurfaceParamDescription = "Pavement type of the street";


        //static readonly string name

        public ParamService()
        {
            this.SetupFile(ParamFile);
        }

        private void SetupFile(string filename)
        {
            try
            {
                string directory = System.IO.Path.GetDirectoryName(filename);
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                if (System.IO.File.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }

                var file = System.IO.File.Create(filename);
                file.Close();
                file.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error setting up shared file: " + e.Message);
            }
        }

        private void CreateStreetParameters(Document doc)
        {
            // Categories
            CategorySet catSet = new CategorySet();
            Category cat = Category.GetCategory(doc, BuiltInCategory.OST_Roads);
            catSet.Insert(cat);

            Binding binding = new InstanceBinding(catSet);

            var app = doc.Application;
            app.SharedParametersFilename = ParamFile;
            DefinitionFile defFile = app.OpenSharedParameterFile();
            DefinitionGroup defGroup = defFile.Groups.FirstOrDefault(d => d.Name == defGroupName);

            if (defGroup == null)
            {
                defGroup = defFile.Groups.Create(defGroupName);
            }

            var options = new ExternalDefinitionCreationOptions(nameParamName, ParameterType.Text);
            options.GUID = new Guid(nameParamGUID);
            options.Description = nameParamDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(streetUsageParamName, ParameterType.Text);
            options.GUID = new Guid(streetUsageParamGUID);
            options.Description = streetUsageDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(streetTypeParamName, ParameterType.Text);
            options.GUID = new Guid(streetTypeParamGUID);
            options.Description = streetTypeDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(streetLanesParam, ParameterType.Integer);
            options.GUID = new Guid(streetLanesParamGUID);
            options.Description = streetLanesDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(streetMaxSpeedParam, ParameterType.Integer);
            options.GUID = new Guid(streetMaxSpeedParamGUID);
            options.Description = streetMaxSpeedDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(streetSurfaceParam, ParameterType.Text);
            options.GUID = new Guid(streetSurfaceParamGUID);
            options.Description = streetSurfaceParamDescription;
            CreateParam(doc, binding, defGroup, options);
        }

        private void CreateParam(Document doc, Binding binding, DefinitionGroup defGroup, ExternalDefinitionCreationOptions options)
        {
            var definition = this.CreateDefinition(defGroup, options);
            doc.ParameterBindings.Insert(definition, binding, BuiltInParameterGroup.PG_IDENTITY_DATA);
        }

        private Definition CreateDefinition(DefinitionGroup defGroup, ExternalDefinitionCreationOptions options)
        {
            Definition definition = defGroup.Definitions.FirstOrDefault(d => d.Name == options.Name);
            if (definition == null)
            {
                definition = defGroup.Definitions.Create(options);
            }

            return definition;
        }



    }
}
