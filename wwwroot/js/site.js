$(document).ready(function () {
    if ($.fn.DataTable) {

        // ============================================================
        // TÜRKÇE + BÜYÜK/KÜÇÜK HARF DUYARSIZ ARAMA
        //
        // Sorun: DataTables'ın kendi iç filtresi term.toLowerCase()
        // kullanır. Standart JS toLowerCase() Türkçe'de hatalı:
        //   "İ".toLowerCase() → "i̇"  (yanlış)
        //   "I".toLowerCase() → "i"   (Türkçe'de "ı" olmalı)
        //
        // Çözüm (iki adım):
        //  1) ext.type.search.string → hücre verileri indekslenirken
        //     trToLower() ile normalize edilir.
        //  2) Arama kutusu dinlenerek kullanıcının yazdığı terim de
        //     trToLower() ile normalize edilip api.search() ile
        //     DataTables'a iletilir. Her iki taraf aynı biçimde
        //     küçük harfe çevrildiğinden eşleşme doğru çalışır.
        // ============================================================
        function trToLower(str) {
            if (str === null || str === undefined) return '';
            return String(str)
                .replace(/İ/g, 'i')
                .replace(/I/g, 'ı')
                .replace(/Ğ/g, 'ğ')
                .replace(/Ü/g, 'ü')
                .replace(/Ş/g, 'ş')
                .replace(/Ö/g, 'ö')
                .replace(/Ç/g, 'ç')
                .toLocaleLowerCase('tr-TR');
        }

        // Adım 1: Hücre verilerini indeks anında Türkçe-farkında normalize et
        $.fn.dataTable.ext.type.search.string = function (data) {
            return trToLower(data);
        };

        // Adım 2: Tabloları başlat
        //   caseInsensitive: false → normalizasyonu biz yapıyoruz,
        //   DataTables'ın kendi toLowerCase() çağrısına gerek yok.
        $('.data-table').DataTable({
            "aLengthMenu": [
                [10, 25, 50, 100, -1],
                [10, 25, 50, 100, "Tümü"]
            ],
            "iDisplayLength": 10,
            "language": {
                "emptyTable":   "Tabloda herhangi bir veri mevcut değil",
                "info":         "_TOTAL_ kayıttan _START_ - _END_ arasındaki kayıtlar gösteriliyor",
                "infoEmpty":    "Kayıt yok",
                "infoFiltered": "(_MAX_ kayıt içerisinden filtrelendi)",
                "infoPostFix":  "",
                "infoThousands":".",
                "lengthMenu":   "Sayfada _MENU_ kayıt göster",
                "loadingRecords":"Yükleniyor...",
                "processing":   "İşleniyor...",
                "search":       "Ara:",
                "zeroRecords":  "Eşleşen kayıt bulunamadı",
                "paginate": {
                    "first":    "İlk",
                    "last":     "Son",
                    "next":     "Sonraki",
                    "previous": "Önceki"
                },
                "aria": {
                    "sortAscending":  ": artan sütun sıralamasını aktifleştir",
                    "sortDescending": ": azalan sütun sıralamasını aktifleştir"
                }
            },
            "search": { "caseInsensitive": false }
        });

        // Adım 3: Her tablo için arama kutusunu ele geçir.
        //   DataTables'ın kendi keyup dinleyicisini kaldırıp yerine
        //   trToLower() normalize edilmiş arama terimini gönderen
        //   dinleyicimizi takıyoruz.
        $('.data-table').each(function () {
            var api         = $(this).DataTable();
            var searchInput = $(this)
                                .closest('.dataTables_wrapper')
                                .find('input[type="search"]');

            // Önce DataTables'ın kendi eventlerini temizle
            searchInput.off('keyup.DT search.DT input.DT paste.DT cut.DT');

            // Bizim normalize eden dinleyicimizi bağla
            searchInput.on('keyup input paste cut', function () {
                var normalized = trToLower($(this).val());
                // regex=false, smart=false → düz substring araması
                api.search(normalized, false, false).draw();
            });
        });
    }

    // ============================================================
    // THEME CUSTOMIZER (SIDEBAR & HEADER SKINS) WITH LOCALSTORAGE
    // ============================================================
    var navbarClasses  = "navbar-purple navbar-primary navbar-success navbar-warning navbar-danger navbar-info navbar-dark navbar-light";
    var sidebarClasses = "sidebar-purple sidebar-primary sidebar-success sidebar-warning sidebar-danger sidebar-info sidebar-dark sidebar-light";

    function applyHeaderSkin(skinClass) {
        $(".navbar").removeClass(navbarClasses).addClass(skinClass);
        $(".header-color-tiles .tiles").removeClass("selected");
        $(".header-color-tiles .tiles[data-header='" + skinClass + "']").addClass("selected");

        // Logo switcher (dark header vs light header)
        if (skinClass === "navbar-light") {
            $(".brand-logo img").attr("src", "/images/logo-latte.svg");
        } else {
            $(".brand-logo img").attr("src", "/images/logo-dark.svg");
        }
        localStorage.setItem("ipgroups_header_skin", skinClass);
    }

    function applySidebarSkin(skinClass) {
        $("body").removeClass(sidebarClasses).addClass(skinClass);
        $(".sidebar-color-tiles .tiles").removeClass("selected");
        $(".sidebar-color-tiles .tiles[data-sidebar='" + skinClass + "']").addClass("selected");
        localStorage.setItem("ipgroups_sidebar_skin", skinClass);
    }

    // Header tile click
    $(document).on("click", ".header-color-tiles .tiles", function () {
        var headerClass = $(this).data("header");
        if (headerClass) applyHeaderSkin(headerClass);
    });

    // Sidebar tile click
    $(document).on("click", ".sidebar-color-tiles .tiles", function () {
        var sidebarClass = $(this).data("sidebar");
        if (sidebarClass) applySidebarSkin(sidebarClass);
    });

    // Load saved preferences on init
    var savedHeader  = localStorage.getItem("ipgroups_header_skin")  || "navbar-purple";
    var savedSidebar = localStorage.getItem("ipgroups_sidebar_skin") || "sidebar-purple";
    applyHeaderSkin(savedHeader);
    applySidebarSkin(savedSidebar);
});
