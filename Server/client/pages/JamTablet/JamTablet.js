
Session.setDefault('trackFilters', {});

Template.JamTablet.helpers({
    tracks: function () {
        return JamTracks.find();
    },
    displayTrack: function(track){
        var filters = Session.get('trackFilters');
        return Object.keys(filters).length === 0 || filters[track.zouzou];
    },
    zouzous: function(){
        return Zouzous.find({jamId: Session.get("jamId")});
    },
    trackName: function(path){
        return path.replace(/_/g, ' ')
            .replace(/\..*$/g, ' ')
            .replace(/120BPM/g, '')
            .replace(/[0-9]*/g, '')
            .trim()
            .replace(/(\/.*\/)*/g, '');
    }
});

Template.jam.events({
    'click .zouzouItem': function(e, tmpl){
        var id = $(e.currentTarget).attr('data-id');
        var filters = Session.get('trackFilters');

        if( filters[id]){
            filters[id] = undefined;
            $(e.currentTarget).velocity('stop').velocity({
                properties:{
                    borderRadius : '50%'
                }, options:{
                    duration: 100,
                    complete: function(){
                        $(e.currentTarget).css('border-radius', '50%');
                    }
                }
            })
        }else{
            filters[id] = true;
            $(e.currentTarget).velocity('stop').velocity({
                properties:{
                    borderRadius : [0, '50%']
                }, options:{
                    duration: 100
                }
            })
        }
        Session.set('trackFilters', filters);
    }
});