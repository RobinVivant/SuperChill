
Meteor.methods({
    hiBitch: function (yo) {
        this.unblock();

        console.log(yo);
        Samples.insert({
            title: yo,
            duration:42
        });
        return yo;
    }
});