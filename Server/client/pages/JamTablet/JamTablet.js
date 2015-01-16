
Template.JamTablet.helpers({
    tracks: function () {
        return JamTracks.find();
    },
    zouzous: function(){
        return Zouzous.find({jamId: Session.get("jamId")});
    }
});

Template.jam.events({

});