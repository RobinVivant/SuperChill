
Router.configure({
    layoutTemplate: 'defaultLayout' ,
    loadingTemplate: 'loading'
});

Router.route('jam',{
    template: 'jam',
    path: '/:jamId?',
    waitOn: function () {

        Session.set("jamId", this.params.jamId);
        var that = this;
        var sub = [
            Meteor.subscribe('samples'),
            Meteor.subscribe('jamList')
        ];

        return sub;
    }
});