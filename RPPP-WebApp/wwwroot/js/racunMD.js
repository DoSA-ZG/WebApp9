$(document).on('click', '.deleterow', function () {
    event.preventDefault();
    var tr = $(this).parents("tr").get(0);

    var value = Number(document.getElementById("zadnji-count").getAttribute("value"));
    document.getElementById("zadnji-count").setAttribute("value", value - 1);

    var count = $($($(tr).children("#idColumn")).children("#count")).val();

    var tbody = $(tr).parents("tbody").get(0);
    var rows = $(tbody).children("tr");
    var numSudionika = rows.length - 2;

    for (let i = Number(Number(count) + 1); i < numSudionika; ++i) {
        var row = $(rows).get(i);
        var columns = $(row).children("td");

        var column = $(columns).get(0);
        var inputs = $(column).children("input");
        $(inputs).get(0).setAttribute("value", i - 1);
        $(inputs).get(1).setAttribute("name", changeName($(inputs).get(1).getAttribute("name"), i - 1, i));
        $(inputs).get(2).setAttribute("name", changeName($(inputs).get(1).getAttribute("name"), i - 1, i));
        
        for (let j = 1; j < 4; ++j) {
            column = $(columns).get(j);
            var input = $($(column).children("input")).get(0);
            console.log("input: " + input);
            console.log("i: " + i);
            input.setAttribute("name", changeName(input.getAttribute("name"), i - 1, i));
        }

        column = $(columns).get(4);
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

    $("#transakcija-dodaj").click(function () {
        event.preventDefault();
        dodajTransakciju();
    });
});

function changeName(oldName, index, oldIndex) {
    console.log("oldName: " + oldName);
    console.log("index: " + index);
    if (oldName === undefined) {
        console.log("undefined");
        return undefined;
    }
    return oldName.replace(index + 1, index);
}

function dodajTransakciju() {
    var sifra = $("#zadnji-count").val(); //+

    console.log("ok");

    var vanjskiIban = $("#transakcija-vanjskiIban").val();
    var unutarnji = $("#transakcija-unutarnji").val();
    var unutarnjiId = $("#transakcija-unutarnjiId").val();
    var iznos = $("#transakcija-iznos").val();
    var vrsta = $("#transakcija-vrsta").val();
    var vrstaId = $("#transakcija-vrstaId").val();
    var smjer = $("#transakcija-smjer").val();


    var template = $('#template').html();

    template = template.replace(/--sifra--/g, sifra)
        .replace(/--vanjskiIban--/g, vanjskiIban)
        .replace(/--unutarnji--/g, unutarnji)
        .replace(/--unutarnjiId--/g, unutarnjiId)
        .replace(/--iznos--/g, iznos)
        .replace(/--vrsta--/g, vrsta)
        .replace(/--vrstaId--/g, vrstaId)
        .replace(/--smjer--/g, smjer)
    $(template).find('tr').insertBefore($("#table-transakcije").find('tr').last());

    var novaSifra = parseInt(sifra) + 1;

    $("#zadnji-count").get(0).setAttribute("value", novaSifra);
    $("#transakcija-vanjskiIban").val('');
    $("#transakcija-unutarnji").val('');
    $("#transakcija-unutarnjiId").val('');
    $("#transakcija-iznos").val('');
    $("#transakcija-vrsta").val('');
    $("#transakcija-vrstaId").val('');
    $("#transakcija-smjer").val('');


    clearOldMessage();
}
