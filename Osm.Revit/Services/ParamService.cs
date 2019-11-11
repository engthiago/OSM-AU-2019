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

        // Shared

        static readonly string nameParamName = "Name";
        static readonly string nameParmGUID = "E513F3CE-5784-4936-AB26-CBE1C2212CB3";

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

        static readonly string streetTypeParamName = "Type";
        static readonly string streetTypeParamGUID = "2F314F17-DB21-430C-BD3F-C6B7029C28F9";

        static readonly string streetLanesParam = "Lanes";
        static readonly string streetLanesParamGUID = "965B8243-1D52-4E74-BDF7-62E9437615E7";

        static readonly string streetMaxSpeedParam = "Max Speed";
        static readonly string streetMaxSpeedParamGUID = "F1195882-5C16-4B35-8D73-35E8F3C05A42";

        static readonly string streetSurfaceParam = "Surface";
        static readonly string streetSurfaceParamGUID = "7F558884-6311-4186-8CCC-CA4E9989469B";


        //static readonly string name

        public ParamService()
        {
        }


    }
}
