
Meteor.methods({
    resetDB: function(){
        JamTracks.remove({});
        TrackGroups.remove({});
        Jam.remove({});
        Jam.insert({
            name: "Jam 1"
        });
        Jam.insert({
            name: "Jam 2"
        });
        Jam.insert({
            name: "Jam 3"
        });
        Jam.insert({
            name: "Jam 4"
        });
        Jam.insert({
            name: "Jam 5"
        });
        Jam.insert({
            name: "Jam 6"
        });
        Jam.insert({
            name: "Jam 7"
        });
        Jam.insert({
            name: "Jam 8"
        });
        Zouzous.remove({});
    }
});