using RPPP_WebApp.Models;
using RPPP_WebApp.Models.DTO;

namespace RPPP_WebApp.Mapper
{
    public class VrstaTransakcijeMapper
    {
        public static VrstaTransakcije ToVrstaTransakcije(VrstaTransakcijePostDTO dto)
        {
            return new VrstaTransakcije
            {
                Naziv = dto.Naziv,
                IdvrstaTransakcije = 0
            };
        }

        public static VrstaTransakcijeGetDTO ToGetDTO(VrstaTransakcije vrstaTransakcije)
        {
            return new VrstaTransakcijeGetDTO
            {
                Naziv = vrstaTransakcije.Naziv,
                IdVrstaTransakcije = vrstaTransakcije.IdvrstaTransakcije
            };
        }

        public static VrstaTransakcije ToVrstaTransakcije(VrstaTransakcijeUpdateDTO dto)
        {
            return new VrstaTransakcije
            {
                Naziv = dto.Naziv,
                IdvrstaTransakcije = dto.IdVrstaTransakcije
            };
        }
    }
}