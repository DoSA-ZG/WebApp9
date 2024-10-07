namespace RPPP_WebApp.Models.DTO
{
    /// <summary>
    /// DTO (Data Transfer Object) koji predstavlja podatke o vrsti zadatka za ažuriranje.
    /// </summary>
    public class VrstaZadatkaUpdateDTO
    {
        /// <summary>
        /// Jedinstveni identifikator vrste zadatka.
        /// </summary>
        public int IdVrstaZdtk { get; set; }

        /// <summary>
        /// Naziv vrste zadatka.
        /// </summary>
        public string NazivVrsteZdtk { get; set; }
    }
}
