
Session.setDefault('trackFilters', {});
Session.setDefault('tracksToGroup', {});
Session.setDefault("tracksGroups", []);

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
        return $(window).height()-160+'px';
    },
    checkedTrack: function(){
        var tracks = Session.get('tracksToGroup');
        return tracks[this._id] ? "checkedTrack" : "";
    },
    tracksGroups: function(){
        return Session.get("tracksGroups");
    },
    isGroupSelected: function(){
        if( Session.get('selectedGroup') === this.color )
            return 'trackGroupSelected';
    },
    getSelectedGroupColor: function(){
        return Session.get('selectedGroup');
    },
    shouldDisplayEffects: function(){
        return Session.get('selectedGroup');
    }
});

Template.JamTablet.events({
    'click .leftPanel .trackItem': function(e, tmpl){
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
                });
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
                });
            }
            trs[this._id] = true;
        }
        Session.set('tracksToGroup', trs);
    },
    'click .createGroupButton': function(e, tmpl){
        var grps = Session.get("tracksGroups");
        grps.push({color: Random.hexString(6), name: chance.first(), tracks: Session.get('tracksToGroup')});
        Session.set("tracksGroups", grps);
        Session.set('tracksToGroup', {});
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
    },
    'click .trackGroup': function(){
        Session.set('selectedGroup', this.color);
    }
});

Template.JamTablet.created = function(){

};