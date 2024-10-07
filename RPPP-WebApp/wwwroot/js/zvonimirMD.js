$(document).on('click', '.deleterow', function () {
    event.preventDefault();
    var tr = $(this).parents("tr").get(0);

    var value = Number(document.getElementById("zadnji-count").getAttribute("value"));
    document.getElementById("zadnji-count").setAttribute("value", value - 1);

    var count = $($($(tr).children("#idColumn")).children("#count")).val();

    var tbody = $(tr).parents("tbody").get(0);
    var rows = $(tbody).children("tr");
    var numSudionika = rows.length - 1;


    tr.remove();
    clearOldMessage();
});

$(function () {
    $(".form-control").bind('keydown', function (event) {
        if (event.which === 13) {
            event.preventDefault();
        }
    });

    $("#dokument-dodaj").click(function () {
        event.preventDefault();
        dodajDokument();
    });
    $("#posao-dodaj").click(function () {
        event.preventDefault();
        dodajPosao();
    });

    $("#uloga-dodaj").click(function () {
        event.preventDefault();
        dodajUlogu();
    });
});

function changeName(oldName, index, oldIndex) {
    return oldName.replace(index + 1, index);
}
function dodajDokument() {
    var sifra = $("#zadnji-count").val(); 

    console.log("ok");

    var naslov = $("#dokumenti-naslov").val();                      
    var sadrzaj = $("#dokumenti-sadrzaj").val();
    var kategorija = $("#dokument-kategorijaDokumenta").val();
    var kategorijaId = $("#dokument-kategorijaDokumentaId").val();

    console.log(`naslov: ${naslov}, sadrzaj: ${sadrzaj}, kategorija: ${kategorija}, kategorijaId: ${kategorijaId}`);

    var template = $('#template').html();
    template = template.replace(/--sifra--/g, sifra)
        .replace(/--naslov--/g, naslov)
        .replace(/--sadrzaj--/g, sadrzaj)
        .replace(/--kategorijaDokumenta--/g, kategorija)
        .replace(/--kategorijaDokumentaId--/g, kategorijaId)
    $(template).find('tr').insertBefore($("#table-dokumenti").find('tr').last());

    var novaSifra = parseInt(sifra) + 1;

    $("#zadnji-count").get(0).setAttribute("value", novaSifra);
    $("#dokumenti-naslov").val('');
    $("#dokumenti-sadrzaj").val('');
    $("#dokument-kategorijaDokumenta").val('');
    $("#dokument-kategorijaDokumentaId").val('');

    clearOldMessage();
}

function dodajPosao() {
    var sifra = $("#posao-zadnji-count").val();

    console.log("ok");

    var naziv = $("#poslovi-naziv").val();
    var pocetak = $("#poslovi-ocekivaniPocetak").val();
    var zavrsetak = $("#poslovi-ocekivaniZavrsetak").val();
    var budzet = $("#poslovi-budzet").val();
    var vrstaPosla = $("#posao-vrstaPosla").val();
    var vrstaPoslaId = $("#posao-vrstaPoslaId").val();

    console.log(`naslov: ${naziv}, pocetak: ${pocetak}, zavrsetak: ${zavrsetak}, budzet: ${budzet}, vrstaPosla: ${vrstaPosla}, vrstaPoslaId: ${vrstaPoslaId}`);

    var template = $('#posao-template').html();
    template = template.replace(/--sifra--/g, sifra)
        .replace(/--naziv--/g, naziv)
        .replace(/--pocetak--/g, pocetak)
        .replace(/--zavrsetak--/g, zavrsetak)
        .replace(/--budzet--/g, budzet)
        .replace(/--vrstaPosla--/g, vrstaPosla)
        .replace(/--vrstaPoslaId--/g, vrstaPoslaId)
    $(template).find('tr').insertBefore($("#table-poslovi").find('tr').last());

    var novaSifra = parseInt(sifra) + 1;

    $("#posao-zadnji-count").get(0).setAttribute("value", novaSifra);
    $("#poslovi-naziv").val('');
    $("#poslovi-ocekivaniPocetak").val('');
    $("#poslovi-ocekivaniZavrsetak").val('');
    $("#poslovi-budzet").val('');
    $("#posao-vrstaPosla").val('');
    $("#posao-vrstaPoslaId").val('');

    clearOldMessage();
}

function dodajUlogu() {
    var sifra = $("#uloga-zadnji-count").val();

    console.log("ok");

    var osoba = $("#ulogaNaProjektu-osoba").val();
    var osobaId = $("#ulogaNaProjektu-idOsoba").val();
    var uloga = $("#ulogaNaProjektu-uloga").val();
    var ulogaId = $("#ulogaNaProjektu-idUloga").val();

    console.log(`osoba: ${osoba}, osobaId: ${osobaId}, uloga: ${uloga}, ulogaId: ${ulogaId}`);

    var template = $('#uloga-template').html();
    template = template.replace(/--sifra--/g, sifra)
        .replace(/--osoba--/g, osoba)
        .replace(/--idOsoba--/g, osobaId)
        .replace(/--uloga--/g, uloga)
        .replace(/--idUloga--/g, ulogaId)
    $(template).find('tr').insertBefore($("#table-uloge").find('tr').last());

    var novaSifra = parseInt(sifra) + 1;

    $("#uloga-zadnji-count").get(0).setAttribute("value", novaSifra);
    $("#ulogaNaProjektu-osoba").val('');
    $("#ulogaNaProjektu-idOsoba").val('');
    $("#ulogaNaProjektu-uloga").val('');
    $("#ulogaNaProjektu-idUloga").val('');

    clearOldMessage();
}
