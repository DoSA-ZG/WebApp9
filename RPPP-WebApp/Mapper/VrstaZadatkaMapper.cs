using RPPP_WebApp.Models;
using RPPP_WebApp.Models.DTO;

namespace RPPP_WebApp.Mapper
{
    /// <summary>
    /// Klasa koja pruža metode za mapiranje između DTO-a i entiteta klase VrstaZadatka.
    /// </summary>
    public class VrstaZadatkaMapper
    {
        /// <summary>
        /// Pretvara VrstaZadatkaPostDTO u entitet klase VrstaZadatka.
        /// </summary>
        /// <param name="dto">DTO objekt koji se pretvara.</param>
        /// <returns>Entitet klase VrstaZadatka.</returns>
        public static VrstaZadatka ToVrstaZadatka(VrstaZadatkaPostDTO dto)
        {
            return new VrstaZadatka
            {
                NazivVrsteZdtk = dto.NazivVrsteZdtk,
                IdVrstaZdtk = 0
            };
        }

        /// <summary>
        /// Pretvara entitet klase VrstaZadatka u DTO objekt.
        /// </summary>
        /// <param name="vrstaZadatka">Entitet klase VrstaZadatka koji se pretvara.</param>
        /// <returns>DTO objekt klase VrstaZadatkaGetDTO.</returns>
        public static VrstaZadatkaGetDTO ToGetDTO(VrstaZadatka vrstaZadatka)
        {
            return new VrstaZadatkaGetDTO
            {
                NazivVrsteZdtk = vrstaZadatka.NazivVrsteZdtk,
                IdVrstaZdtk = vrstaZadatka.IdVrstaZdtk
            };
        }

        /// <summary>
        /// Pretvara VrstaZadatkaUpdateDTO u entitet klase VrstaZadatka.
        /// </summary>
        /// <param name="dto">DTO objekt koji se pretvara.</param>
        /// <returns>Entitet klase VrstaZadatka.</returns>
        public static VrstaZadatka ToVrstaZadatka(VrstaZadatkaUpdateDTO dto)
        {
            return new VrstaZadatka
            {
                NazivVrsteZdtk = dto.NazivVrsteZdtk,
                IdVrstaZdtk = dto.IdVrstaZdtk
            };
        }
    }
}
