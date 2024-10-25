function initializePagination(currentPage, totalPages, prefecturesArea, typeValue, page_size) {
    ClubPagination(currentPage, totalPages, prefecturesArea, typeValue, page_size);
    $('#prev-btn').on('click', function () {
        if (currentPage > 1) {
            changePage(currentPage - 1, totalPages, prefecturesArea, typeValue, page_size);
        }
    });
    $('#next-btn').on('click', function () {
        if (currentPage < totalPages) {
            changePage(currentPage + 1, totalPages, prefecturesArea, typeValue, page_size);
        }
    });
}
function ClubPagination(currentPage, totalPages, prefecturesArea, typeValue, page_size) {
    $('#page-numbers').empty();
    $('#prev-btn').prop('disabled', currentPage === 1);
    let startPage = Math.max(1, currentPage - 2);
    let endPage = Math.min(totalPages, currentPage + 2);

    if (currentPage - 2 < 1) {
        endPage = Math.min(5, totalPages);
    }
    if (currentPage + 2 > totalPages) {
        startPage = Math.max(1, totalPages - 4);
    }
    if (startPage > 1) {
        $('#page-numbers').append(createPageElement(1, prefecturesArea, typeValue, page_size));
        //if (startPage > 2) {
        //    $('#page-numbers').append(createEllipsis());
        //}
    }
    for (let i = startPage; i <= endPage; i++) {
        const pageElement = createPageElement(i, prefecturesArea, typeValue, page_size);
        if (i === currentPage) {
            pageElement.addClass('active');
        }
        $('#page-numbers').append(pageElement);
    }
    //if (endPage < totalPages) {
    //    if (endPage < totalPages - 1) {
    //        $('#page-numbers').append(createEllipsis());
    //    }
    //    $('#page-numbers').append(createPageElement(totalPages, prefecturesArea, typeValue, page_size));
    //}
    $('#next-btn').prop('disabled', currentPage === totalPages);
}
function createPageElement(page, prefecturesArea, typeValue, page_size) {
    return $('<div>', {
        text: page,
        click: function () {
            changePage(page, totalPages, prefecturesArea, typeValue, page_size);
        }
    });
}
function createEllipsis() {
    return $('<div>', { text: '...' });
}
function changePage(page, totalPages, prefecturesArea, typeValue, page_size) {
    ClubPagination(page, totalPages, prefecturesArea, typeValue, page_size);
    submitForm(page, prefecturesArea, typeValue, page_size);
}

function submitForm(page, prefecturesArea, typeValue, page_size) {
    //const page_size = page_size;
    const startIndex = (page - 1) * page_size;
    $('#startindex-id').val(startIndex);
    const form = document.getElementById("pagination-form-id");
    if (typeValue.trim().toUpperCase() === "CLUB") {
        form.action = `/search${prefecturesArea}/?target=${typeValue}`;
    } else if (typeValue.trim().toUpperCase() === "HOST") {
        form.action = `/search${prefecturesArea}/?target=${typeValue}`;
    } else if (typeValue.trim() === "scftab=01") {
        form.action = `/search${prefecturesArea}/?${typeValue}`;
    } else if (typeValue.trim() === "scftab=02") {
        form.action = `/search${prefecturesArea}/?${typeValue}`;
    } else if (typeValue === "") {
        form.action = `/search${prefecturesArea}`;
    } else if (typeValue.trim() === "scftab=03&NewClub=true") {
        form.action = `/search${prefecturesArea}/?${typeValue}`;
    } else if (typeValue.trim() === "scftab=03&NewHost=true") {
        form.action = `/search${prefecturesArea}/?${typeValue}`;
    } else if (typeValue.trim() === "new") {
        form.action = `/search${prefecturesArea}/?target=new&NewClub=true`;
    }
    else {
        form.action = `/search${prefecturesArea}/?TopSearch=${typeValue}`;
    }
    form.submit();
}
