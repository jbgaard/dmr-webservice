using DMRWebScrapper_service.Code;

namespace DMRWebScrapper_service.Models
{
    public class Bildata
    {
        public DMRKøretøj Køretøj { get; set; }

        public DMRTeknisk Teknisk { get; set; }

        public DMRSyn Syn { get; set; }

        public DMRAfgifter Afgifter { get; set; }

        public DMRForsikring Forsikring { get; set; }

        // VehicleViewReport
        public VehicleViewService.VehicleViewReport? VehicleViewReport { get; set; }

        public Bildata()
        {
            Køretøj = new DMRKøretøj();
            Teknisk = new DMRTeknisk();
            Syn = new DMRSyn();
            Afgifter = new DMRAfgifter();
            Forsikring = new DMRForsikring();
        }
    }

    public class BildataMin
    {
        public DMRKøretøj Køretøj { get; set; }

        public DMRForsikring Forsikring { get; set; }

        // VehicleViewReport
        public VehicleViewService.VehicleViewReport? VehicleViewReport { get; set; }

        public BildataMin()
        {
            Køretøj = new DMRKøretøj();
            Forsikring = new DMRForsikring();
        }
    }
}
