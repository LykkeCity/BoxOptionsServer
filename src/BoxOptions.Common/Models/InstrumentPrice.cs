using BoxOptions.Core.Interfaces;
using System;

namespace BoxOptions.Common.Models
{
    public class InstrumentPrice : Price, IInstrumentPrice
    {
        public string Instrument { get; set; }
        public string Source { get; set; }
        public DateTime ReceiveDate { get; set; }

        public new IInstrumentPrice ClonePrice()
        {
            try
            {
                Price res = base.ClonePrice();
                InstrumentPrice retval = new InstrumentPrice()
                {
                    Instrument = this.Instrument,
                    Source = this.Source,
                    Ask = res.Ask,
                    Bid = res.Bid,
                    Date = res.Date,
                    ReceiveDate = DateTime.UtcNow
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
