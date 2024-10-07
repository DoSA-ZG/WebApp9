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

    $("#suradnik-dodaj").click(function () {
        event.preventDefault();
        dodajSuradnika();
    });
});

function changeName(oldName, index, oldIndex) {
    return oldName.replace(index + 1, index);
}
function dodajSuradnika() {
    var sifra = $("#zadnji-count").val(); //+

        console.log("ok");

        var naziv = $("#suradnici-naziv").val();                        //+
        var oib = $("#suradnici-oib").val();   //+
        var adresa = $("#suradnici-adresa").val();   //+
        var postanskiBroj = $("#suradnici-postanskiBroj").val(); //+    //+
        var grad = $("#suradnici-grad").val();
        var vrsta = $("#suradnik-vrsta").val();
        var vrstaId = $("#suradnik-vrstaId").val();

        //Alternativa ako su hr postavke sa zarezom //http://haacked.com/archive/2011/03/19/fixing-binding-to-decimals.aspx/
        //ili ovo http://intellitect.com/custom-model-binding-in-asp-net-core-1-0/

        var template = $('#template').html();
        template = template.replace(/--sifra--/g, sifra)
            .replace(/--naziv--/g, naziv)
            .replace(/--oib--/g, oib)
            .replace(/--adresa--/g, adresa)
            .replace(/--postanskiBroj--/g, postanskiBroj)
            .replace(/--grad--/g, grad)
            .replace(/--vrstaSuradnika--/g, vrsta)
            .replace(/--vrstaSuradnikaId--/g, vrstaId)
        $(template).find('tr').insertBefore($("#table-suradnici").find('tr').last());

        var novaSifra = parseInt(sifra) + 1;

        $("#zadnji-count").get(0).setAttribute("value", novaSifra);
        $("#suradnici-naziv").val('');                        //+
        $("#suradnici-oib").val('');   //+
        $("#suradnici-adresa").val('');   //+
        $("#suradnici-postanskiBroj").val(''); //+    //+
        $("#suradnici-grad").val('');
        $("#suradnik-vrsta").val('');
        $("#suradnik-vrstaId").val('');

        clearOldMessage();
}