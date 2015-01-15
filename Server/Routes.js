
Router.configure({
    layoutTemplate: 'defaultLayout' ,
    loadingTemplate: 'loading'
});

Router.route('jam',{
    template: 'jam',
    path: '/:jamId?',
    waitOn: function () {
        if( !localStorage.getItem("zouzouId") ){
            localStorage.setItem("zouzouId", Random.hexString(6));
        }
        Session.set("zouzouId",localStorage.getItem("zouzouId"));
        Session.set("jamId", this.params.jamId);
        var that = this;
        var sub = [
            Meteor.subscribe('samples'),
            Meteor.subscribe('jamList'),
            Meteor.subscribe('zouzou', localStorage.getItem("zouzouId"))
        ];

        return sub;
    }
});