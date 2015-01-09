
Router.configure({
    layoutTemplate: 'defaultLayout' /*,
    loadingTemplate: 'loading',
    notFoundTemplate: '404'*/
});

Router.route('/', function () {
    this.render('home');
});

Router.route('/master', function () {
    this.render('master');
});

Router.route('jam',{
    template: 'slave',
    path: '/:jamId',
    waitOn: function () {
        Session.set("jamId", this.params.jamId);
        return [
            Meteor.subscribe('jam', this.params.jamId, {
                onError: function(error){
                    Router.go('/');
                }
            }),
            Meteor.subscribe('samples')
        ];
    },
    data: function() {
        return {jamId: this.params.jamId}
    }

});