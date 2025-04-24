let firstNameData = document.getElementById("firstName");
let lastNameData = document.getElementById("lastName");
let emailData = document.getElementById("email");
let genderData = document.getElementById("gender");



let mobileData = document.getElementById("mobile")

let fullNameData = document.getElementsByClassName("fullName")
let dateOfJoiningData = document.getElementById("dateOfJoin")

let imageData = document.getElementById("yourImg")


if (firstNameData && lastNameData && emailData && genderData) {
    // let localStorageUsers = JSON.parse(localStorage.getItem('users')) || [];
    let sessionCurrentUser = JSON.parse(sessionStorage.getItem('loggedInUser'));


    //double check that we have data in sessionStorage
    if (sessionCurrentUser && sessionCurrentUser.email) {

        if (sessionCurrentUser.email) {
            firstNameData.value = sessionCurrentUser.firstName;
            lastNameData.value = sessionCurrentUser.lastName;

            fullNameData[0].innerHTML = firstNameData.value + " " + lastNameData.value
            fullNameData[1].value = firstNameData.value + " " + lastNameData.value

            emailData.value = sessionCurrentUser.email;

            genderData.value = sessionCurrentUser.gender;


            mobileData.value = sessionCurrentUser.phoneNumber;
            dateOfJoiningData.value = sessionCurrentUser.dateOfJoining || ''

            // imageData.src = sessionCurrentUser.imageSrc || "../Images/pexels-olly-927451.jpg"
            if (sessionCurrentUser.email == "mfawareh1@gmail.com") {
                document.getElementById('yourImg').src = '../Images/aboutimg/mohammad.jpeg'
            }


        }

    } else {
        console.error("No logged in user found in sessionStorage.");
    }
} else {
    console.error("One or more required elements are missing from the DOM.");
}