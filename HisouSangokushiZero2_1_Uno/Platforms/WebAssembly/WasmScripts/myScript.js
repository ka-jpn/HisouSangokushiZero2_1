//document.getElementsByTagName('div').item(0).onwheel = (e) => top.scrollBy(0, e.deltaY);//test
function fixBody(){
    document.getElementById('uno-body').classList.remove('fit');
    document.getElementById('uno-body').classList.add('fix');
}
function fitBody(){
    document.getElementById('uno-body').classList.remove('fix');
    document.getElementById('uno-body').classList.add('fit');
}