
Session.setDefault('trackFilters', {});
Session.setDefault('tracksToGroup', {});

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
    },
    verticalSectionHeight: function(){
        return $(window).height()-150+'px';
    },
    checkedTrack: function(){
        var tracks = Session.get('tracksToGroup');
        return tracks[this._id] ? "checkedTrack" : "";
    }
});

Template.JamTablet.events({
    'click .trackItem': function(e, tmpl){
        var trs = Session.get('tracksToGroup');
        if(trs[this._id]){
            trs[this._id] = undefined;
            if( Object.keys(trs).length === 1 ){
                $('.createGroupButton').velocity('stop').velocity({
                    properties:{
                        opacity: [0, 1]
                    },options:{
                        duration: 200,
                        complete: function(){

                                $('.createGroupButton').hide();

                        }
                    }
                })
            }
        }else{
            if( Object.keys(trs).length === 0 ){
                $('.createGroupButton').show();
                $('.createGroupButton').css('bottom', ($('.leftPanel').height()-50)/2 +'px');
                $('.createGroupButton').css('left', $('.leftPanel').width()-35 +'px');
                $('.createGroupButton').velocity('stop').velocity({
                    properties:{
                        opacity: [1, 0]
                    },options:{
                        duration: 200
                    }
                })
            }
            trs[this._id] = true;
        }
        Session.set('tracksToGroup', trs);
    },
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

Template.JamTablet.created = function(){

};