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
        static readonly string defGroupName = "OSM";

        // Shared

        static readonly string nameParamName = "Name";
        public static string NameParamGUID => "E513F3CE-5784-4936-AB26-CBE1C2212CB3";
        static readonly string nameParamDescription = "The common name for Open Street Map Elements";

        // Building

        static readonly string buildingtypeParamName = "Building Type";
        public static string BuildingTypeParmGUID => "2F7AE11B-BADA-4BC3-8FBA-BD62175521FA";
        static readonly string buildingtypeDescription = "The building type, referred as 'building' by OSM";

        static readonly string streetNumberParamName = "Street Number";
        public static string StreetNumberParamGUID => "47D313E6-AA4E-43A7-A650-5A9AA29B3D08";
        static readonly string streetNumberDescription = "The building street number, referred as 'addr:housenumber' by OSM";

        static readonly string streetNameParamName = "Street Name";
        public static string StreetNameParamGUID => "0A337537-D2AA-4F03-8899-FDE6CE993592";
        static readonly string streetNameDescription = "The building street name, referred as 'addr:street' by OSM";

        static readonly string postalCodeParam = "Postal Code";
        public static string PostalCodeParamGUID => "6E4F7194-A53C-4DD8-85A4-4724650A0025";
        static readonly string postalCodeParamDescription = "The building street postal code, referred as 'addr:postcode' by OSM";

        static readonly string buldingHeightParam = "Height";
        public static string BuildingHeightParamGUID => "F657A6EA-06F3-4F7A-B20B-7F67A13B7027";
        static readonly string buildingHeightParamDescription = "The building height, if specified, referred as 'height' by OSM";

        // Street

        static readonly string streetUsageParamName = "Usage";
        public static string StreetUsageParamGUID => "18C7DEEF-6FDC-4B31-BE2A-F47EC9787D40";
        static readonly string streetUsageDescription = "Usage of the street, referred as 'highway' by OSM";

        static readonly string streetTypeParamName = "Type";
        public static string StreetTypeParamGUID => "2F314F17-DB21-430C-BD3F-C6B7029C28F9";
        static readonly string streetTypeDescription = "Type of the street, referred as 'tiger:name_type' by OSM";

        static readonly string streetLanesParam = "Lanes";
        public static string StreetLanesParamGUID => "965B8243-1D52-4E74-BDF7-62E9437615E7";
        static readonly string streetLanesDescription = "Total lanes of the street";

        static readonly string streetMaxSpeedParam = "Max Speed";
        public static string StreetMaxSpeedParamGUID => "F1195882-5C16-4B35-8D73-35E8F3C05A42";
        static readonly string streetMaxSpeedDescription = "Maximum speed of the street, in Km/h";

        static readonly string streetSurfaceParam = "Surface";
        public static string StreetSurfaceParamGUID => "7F558884-6311-4186-8CCC-CA4E9989469B";
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

        public void CreateBuildingParameters(Document doc)
        {
            // Categories
            CategorySet catSet = new CategorySet();
            Category cat = Category.GetCategory(doc, BuiltInCategory.OST_GenericModel);
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

            var options = new ExternalDefinitionCreationOptions(buildingtypeParamName, ParameterType.Text);
            options.GUID = new Guid(BuildingTypeParmGUID);
            options.Description = buildingtypeDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(streetNumberParamName, ParameterType.Text);
            options.GUID = new Guid(StreetNumberParamGUID);
            options.Description = streetNumberDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(streetNameParamName, ParameterType.Text);
            options.GUID = new Guid(StreetNameParamGUID);
            options.Description = streetNameDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(postalCodeParam, ParameterType.Text);
            options.GUID = new Guid(PostalCodeParamGUID);
            options.Description = postalCodeParamDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(buldingHeightParam, ParameterType.Length);
            options.GUID = new Guid(BuildingHeightParamGUID);
            options.Description = buildingHeightParamDescription;
            CreateParam(doc, binding, defGroup, options);
        }

        public void CreateStreetParameters(Document doc)
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
            options.GUID = new Guid(NameParamGUID);
            options.Description = nameParamDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(streetUsageParamName, ParameterType.Text);
            options.GUID = new Guid(StreetUsageParamGUID);
            options.Description = streetUsageDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(streetTypeParamName, ParameterType.Text);
            options.GUID = new Guid(StreetTypeParamGUID);
            options.Description = streetTypeDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(streetLanesParam, ParameterType.Integer);
            options.GUID = new Guid(StreetLanesParamGUID);
            options.Description = streetLanesDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(streetMaxSpeedParam, ParameterType.Integer);
            options.GUID = new Guid(StreetMaxSpeedParamGUID);
            options.Description = streetMaxSpeedDescription;
            CreateParam(doc, binding, defGroup, options);

            options = new ExternalDefinitionCreationOptions(streetSurfaceParam, ParameterType.Text);
            options.GUID = new Guid(StreetSurfaceParamGUID);
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

        public void SetParameter(Element element, string paramGuid, string value)
        {
            if (value == null) return;
            var param = element.get_Parameter(new Guid(paramGuid));
            if (param == null) return;
            if (param.IsReadOnly) return;

            param.Set(value);
        }

        public void SetParameter(Element element, string paramGuid, double value)
        {
            var param = element.get_Parameter(new Guid(paramGuid));
            if (param == null) return;
            if (param.IsReadOnly) return;

            param.Set(value);
        }

    }
}
