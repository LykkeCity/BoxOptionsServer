using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BoxOptions.Public.Models
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
                if (result.Length<2 || !result.StartsWith("[")|| !result.EndsWith("]"))
                    throw new FormatException("Invalid format");
                string objectstring = result.Substring(1, result.Length - 2);

                string[] blocks = objectstring.Split('[');
                foreach(var block in blocks)
                {
                    CoefBlock newBlock = new CoefBlock();
                    List<Coeff> coefBlockList = new List<Coeff>();
                    if (block.Length < 1)
                        continue;
                    string[] coefs = block.Split('{');                    
                    foreach (var coef in coefs)
                    {
                        if (coef.Length < 1)
                            continue;

                        string coefstring = "";
                        if (coef.EndsWith("},")|| coef.EndsWith("}]"))
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
                            }
                            else if (valuePair[0] == "\"missCoeff\"")
                            {
                                newItem.MissCoeff = decimal.Parse(valuePair[1], ci);                                
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

        
    }
}
