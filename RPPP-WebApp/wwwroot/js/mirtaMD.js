$(document).on('click', '.deleterow', function () {
    event.preventDefault();
    var tr = $(this).parents("tr").get(0);

    var value = Number(document.getElementById("zadnji-count").getAttribute("value"));
    document.getElementById("zadnji-count").setAttribute("value", value - 1);

    var count = $($($(tr).children("#idColumn")).children("#count")).val();

    var tbody = $(tr).parents("tbody").get(0);
    var rows = $(tbody).children("tr");
    var numSudionika = rows.length - 1;

    for (let i = Number(Number(count) + 1); i < numSudionika; ++i) {
        var row = $(rows).get(i);
        var columns = $(row).children("td");

        var column = $(columns).get(0);
        var inputs = $(column).children("input");
        $(inputs).get(0).setAttribute("value", i - 1);
        $(inputs).get(1).setAttribute("name", changeName($(inputs).get(1).getAttribute("name"), i - 1, i));
        $(inputs).get(2).setAttribute("name", changeName($(inputs).get(1).getAttribute("name"), i - 1, i));

        for (let j = 1; j < 5; ++j) {
            column = $(columns).get(j);
            var input = $($(column).children("input")).get(0);
            input.setAttribute("name", changeName(input.getAttribute("name"), i - 1, i));
        }

        column = $(columns).get(5);
        inputs = $(column).children("input");
        var input = $(inputs).get(1);
        input.setAttribute("name", changeName(input.getAttribute("name"), i - 1, i));
    }
    tr.remove();
    clearOldMessage();
});

$(function () {
    $(".form-control").bind('keydown', function (event) {
        if (event.which === 13) {
            event.preventDefault();
        }
    });

    $("#osoba-dodaj").click(function () {
        event.preventDefault();
        dodajOsobu();
    });
});

function changeName(oldName, index, oldIndex) {
    return oldName.replace(index + 1, index);
}
function dodajOsobu() {
    var sifra = $("#zadnji-count").val();

    var ime = $("#osobe-ime").val();
    var prezime = $("#osobe-prezime").val();
    var email = $("#osobe-email").val();
    var telefon = $("#osobe-telefon").val();
    var iban = $("#osobe-iban").val();

    var template = $('#template').html();
    template = template.replace(/--sifra--/g, sifra)
        .replace(/--imeOsobe--/g, ime)
        .replace(/--prezimeOsobe--/g, prezime)
        .replace(/--email--/g, email)
        .replace(/--telefon--/g, telefon)
        .replace(/--IBAN--/g, iban)
    $(template).find('tr').insertBefore($("#table-osobe").find('tr').last());

    var novaSifra = parseInt(sifra) + 1;

    $("#zadnji-count").get(0).setAttribute("value", novaSifra);
    $("#osobe-ime").val('');
    $("#osobe-prezime").val('');
    $("#osobe-email").val('');
    $("#osobe-telefon").val('');
    $("#osobe-iban").val('');

    clearOldMessage();
}