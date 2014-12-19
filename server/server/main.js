

Meteor.methods({
    hiBitch: function (yo) {
        this.unblock();

        console.log(yo);
        return yo;
    }
});