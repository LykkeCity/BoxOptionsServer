using System;

namespace BoxOptions.Core.Models
{
    public class InstrumentPrice:Price
    {
        public string Instrument { get; set; }

        public override Price ClonePrice()
        {
            try
            {
                Price res = base.ClonePrice();
                InstrumentPrice retval = new InstrumentPrice()
                {
                    Instrument = this.Instrument,
                    Ask = res.Ask,
                    Bid = res.Bid,
                    Date = res.Date
                };
                
                return retval;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
