using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Common;

namespace BoxOptions.Services.Models
{
    public class CoefModels
    {
        public class CoefRequestResult
        {
            public List<CoefBlock> CoefBlocks { get; set; }

            public CoefRequestResult()
            {
                CoefBlocks = new List<CoefBlock>();
            }

            internal static CoefRequestResult Parse(string result)
            {
                // Culture info for decimal value conversion.
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-us");

                // return value
                CoefRequestResult retval = new CoefRequestResult();


                List<CoefBlock> blockList = new List<CoefBlock>();
                // Invalid json format
                if (result.Length < 2 || !result.StartsWith("[") || !result.EndsWith("]"))
                    throw new FormatException("Invalid format");
                string objectstring = result.Substring(1, result.Length - 2);

                string[] blocks = objectstring.Split('[');
                foreach (var block in blocks)
                {
                    CoefBlock newBlock = new CoefBlock();
                    List<Coeff> coefBlockList = new List<Coeff>();
                    if (block.Length < 1)
                        continue;
                    string[] coefs = block.Split('{');
                    //Console.WriteLine(coefs);
                    foreach (var coef in coefs)
                    {
                        if (coef.Length < 1)
                            continue;

                        string coefstring = "";
                        if (coef.EndsWith("},") || coef.EndsWith("}]"))
                            coefstring = coef.Substring(0, coef.Length - 2);
                        else if (coef.EndsWith("}],"))
                            coefstring = coef.Substring(0, coef.Length - 3);
                        else
                            throw new FormatException("invalid Format");

                        string[] coefValues = coefstring.Split(',');
                        Coeff newItem = new Coeff();
                        foreach (var item in coefValues)
                        {
                            string[] valuePair = item.Split(':');

                            if (valuePair[0] == "\"hitCoeff\"")
                            {
                                newItem.HitCoeff = decimal.Parse(valuePair[1], ci);
                                //Console.WriteLine("HitCoeff:\n\t{0}\n\t{1:F16}", valuePair[1], newItem.HitCoeff);
                            }
                            else if (valuePair[0] == "\"missCoeff\"")
                            {
                                newItem.MissCoeff = decimal.Parse(valuePair[1], ci);
                                //Console.WriteLine("MissCoeff :\n\t{0}\n\t{1:F16}", valuePair[1], newItem.MissCoeff);
                            }
                        }
                        coefBlockList.Add(newItem);
                    }
                    newBlock.Coeffs = coefBlockList.ToArray();
                    retval.CoefBlocks.Add(newBlock);
                }

                return retval;

            }
        }

        public class CoefBlock
        {
            public Coeff[] Coeffs { get; set; }
        }

        public class Coeff
        {
            [JsonProperty("hitCoeff")]
            public decimal HitCoeff { get; set; }
            [JsonProperty("missCoeff")]
            public decimal MissCoeff { get; set; }
        }

        public static string GetEmptyCoeffs()
        {
            Coeff[][] res = new Coeff[8][];
            for (int i = 0; i < 8; i++)
            {
                res[i] = new Coeff[15];
                for (int j = 0; j < 15; j++)
                {
                    res[i][j] = new Coeff { HitCoeff = 1m, MissCoeff = 1m };
                }   
            }
            return res.ToJson();
        }
    }
}
