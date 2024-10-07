using RPPP_WebApp.Models;
using RPPP_WebApp.Models.DTO;

namespace RPPP_WebApp.Mapper
{
    public class VrstaSuradnikaMapper
    {
        public static VrstaSuradnika ToVrstaSuradnika(VrstaSuradnikaPostDTO dto)
        {
            return new VrstaSuradnika
            {
                Naziv = dto.Naziv,
                Opis = dto.Opis,
                IdVrstaSuradnika = 0
            };
        }

        public static VrstaSuradnikaGetDTO ToGetDTO(VrstaSuradnika vrstaSuradnika)
        {
            return new VrstaSuradnikaGetDTO
            {
                Naziv = vrstaSuradnika.Naziv,
                Opis = vrstaSuradnika.Opis,
                IdVrstaSuradnika = vrstaSuradnika.IdVrstaSuradnika
            };
        }

        public static VrstaSuradnika ToVrstaSuradnika(VrstaSuradnikaUpdateDTO dto)
        {
            return new VrstaSuradnika
            {
                Naziv = dto.Naziv,
                Opis = dto.Opis,
                IdVrstaSuradnika = dto.IdVrstaSuradnika
            };
        }

    }
}
