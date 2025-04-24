let firstNameDataEdit = document.getElementById("firstNameEdit");
let lastNameDataEdit = document.getElementById("lastNameEdit");
let emailDataEdit = document.getElementById("emailEdit");
let mobileDataEdit = document.getElementById("phoneEdit");
let dateOfJoiningEdit = document.getElementById("dateOfJoiningEdit");
let femaleChecked = document.getElementById("FE");
let maleChecked = document.getElementById("MA");
let saveButton = document.getElementById("change");

let sessionCurrentUser = JSON.parse(sessionStorage.getItem('loggedInUser'));

if (sessionCurrentUser) {
    firstNameDataEdit.value = sessionCurrentUser.firstName || '';
    lastNameDataEdit.value = sessionCurrentUser.lastName || '';
    emailDataEdit.value = sessionCurrentUser.email || '';
    mobileDataEdit.value = sessionCurrentUser.phoneNumber || '';
    dateOfJoiningEdit.value = sessionCurrentUser.dateOfJoining || '';

    if (sessionCurrentUser.gender === 'Female') {
        femaleChecked.checked = true;  //keeps the check visible to the user
    } else if (sessionCurrentUser.gender === 'Male') {
        maleChecked.checked = true; //keeps the check visible to the user
    }
}

saveButton.addEventListener('click', function () {
    // event.preventDefault(); // Prevent the form from submitting

    let localStorageUsers = JSON.parse(localStorage.getItem('users')) || [];

    for (let i = 0; i < localStorageUsers.length; i++) {
        if (sessionCurrentUser.email === localStorageUsers[i].email) {
            localStorageUsers[i].firstName = firstNameDataEdit.value || localStorageUsers[i].firstName;
            localStorageUsers[i].lastName = lastNameDataEdit.value || localStorageUsers[i].lastName;
            localStorageUsers[i].email = emailDataEdit.value || localStorageUsers[i].email;
            localStorageUsers[i].phoneNumber = mobileDataEdit.value || localStorageUsers[i].phoneNumber;
            localStorageUsers[i].dateOfJoining = dateOfJoiningEdit.value || localStorageUsers[i].dateOfJoining;

            // Update gender
            if (femaleChecked.checked) {
                localStorageUsers[i].gender = femaleChecked.value;
            } else if (maleChecked.checked) {
                localStorageUsers[i].gender = maleChecked.value;
            }

            // Save the updated users array back to localStorage
            localStorage.setItem('users', JSON.stringify(localStorageUsers));
            sessionStorage.setItem('loggedInUser', JSON.stringify(localStorageUsers[i]));

            // window.location.href = 'profilePage0.html'
            break; // Exit the loop once the user is found
        }
    }
});