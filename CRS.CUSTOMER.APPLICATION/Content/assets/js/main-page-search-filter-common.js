function LoadSavedData() {
   // EnableLoaderFunction();
    document.body.classList.add('body-no-scroll');
    var CustomerCurrentLocationId = $('#current-location-id').val();
    var preferencefilterpopupContent = $('#tab1').html();
    debugger
    if (preferencefilterpopupContent.trim() !== '') {
        var element = document.getElementById('drawer-filter-location');
        if (element) {

            element.classList.remove('translate-y-full');
            DisableLoaderFunction();
            return false;
        }
    }
    var savedData = localStorage.getItem('ClubFilterHTMLContent');
    if (savedData) {
        savedData = JSON.parse(savedData);
        document.getElementById('tab1').innerHTML = savedData.content; // Render the HTML content


        // Set input field values
        for (var inputId in savedData.inputValues) {
            if (inputId != "" && inputId != '') {
                if (document.getElementById(inputId)) {
                    document.getElementById(inputId).value = savedData.inputValues[inputId];
                }
            }
        }

        // Set checkbox states
        for (var checkboxId in savedData.checkboxStates) {
            if (document.getElementById(checkboxId)) {
                document.getElementById(checkboxId).checked = savedData.checkboxStates[checkboxId];
            }
        }

        for (var dropdownId in savedData.dropdownValues) {
            var selectDropdown = document.getElementById(dropdownId);
            if (selectDropdown) {
                for (var i = 0; i < selectDropdown.options.length; i++) {
                    if (selectDropdown.options[i].value === savedData.dropdownValues[dropdownId]) {
                        selectDropdown.options[i].selected = true;
                        break;
                    }
                }
            }
        }
        var ClubDetailMapData = localStorage.getItem('ClubDetailMapData');
        if (ClubDetailMapData) {
            ClubDetailMapData = JSON.parse(ClubDetailMapData);
            LoadGoogleMaps(ClubDetailMapData);
        }
        $('#drawer-filter-location').css('display', 'none');
        DisableLoaderFunction1();
        return false;
    }
}

function ManageMainPageSearchHTMLContent() {
    var content = document.getElementById('tab1').innerHTML;
    var inputValues = {};
    var checkboxStates = {};
    var dropdownValues = {};
    document.querySelectorAll('input, select').forEach(function (input) {
        if (input.tagName === 'INPUT') {
            if (input.type === 'text' || input.type === 'search') {
                inputValues[input.id] = input.value; // Store input field values
            } else if (input.type === 'checkbox') {
                checkboxStates[input.id] = input.checked; // Store checkbox states
            }
        }
        else if (input.tagName === 'SELECT') {
            dropdownValues[input.id] = input.value; // Store dropdown values
        }
    });
    var savedData = {
        content: content,
        inputValues: inputValues,
        checkboxStates: checkboxStates,
        dropdownValues: dropdownValues
    };
    //localStorage.setItem('PreferenceFilterHTMLContent', content);
    localStorage.setItem('ClubFilterHTMLContent', JSON.stringify(savedData)); // Store the data
}
function getSelectedCheckboxValues(checkboxName, targetElementId, checkboxClass) {
    var checkboxes = document.querySelectorAll('input[type ="checkbox" ][name="' + checkboxName + '" ]');
    var selectedValues = [];
    checkboxes.forEach(function (checkbox) {
        if (checkbox.checked && checkbox.classList.contains(checkboxClass)) {
            selectedValues.push(checkbox.value);
        }
    });
    document.getElementById(targetElementId).value = selectedValues.join(', ');
}
function DisableLoaderFunction1() {
    document.getElementById('loader-id-v2').style.display = 'none';
    document.body.removeEventListener('touchmove', preventDefault, { passive: false });
    document.body.classList.remove('no-scroll-loader');
}
