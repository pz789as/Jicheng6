/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 * @flow
 */

import React, { Component } from 'react';
import {
  AppRegistry,
  StyleSheet,
  Text,
  View,
  PanResponder,
  TouchableOpacity,
  StatusBar,
  ART,
} from 'react-native';

const {
  Shape,
  Group,
  Transform,
  Surface,
  Path,
  Pattern,
  LinearGradient,
  RadialGradient,
  // Text,
  ClippingRectangle,
} = ART;

import Dimensions from 'Dimensions';
let ScreenWidth = Dimensions.get('window').width;
global.ScreenWidth = global.ScreenWidth || ScreenWidth;
let ScreenHeight = Dimensions.get('window').height;
global.ScreenHeight = global.ScreenHeight || ScreenHeight;
console.log(ScreenWidth, ScreenHeight);

let curWidth = Math.min(ScreenWidth, ScreenHeight);
global.curWidth = global.curWidth || curWidth;
let scaleWidth = curWidth / 400;
global.scaleWidth = global.scaleWidth || scaleWidth;
let minUnit = ScreenWidth / 100;
global.minUnit = global.minUnit || minUnit;
let unitDisSt = curWidth / 25;
let unitDisMv = curWidth / 15;

import DrawWord from './DrawWord1';

var data = require('./data/瞅.json');

let Dis = function(x, y){
  return Math.sqrt(x * x + y * y);
};
global.Dis = global.Dis || Dis;
let DisP = function(p1, p2){
  return Dis(p1.x - p2.x, p1.y - p2.y);
};
global.DisP = global.DisP || DisP;
let Lerp = function(a, b, f){
  if (f <= 0) return a;
  if (f >= 1) return b;
  return a + (b - a) * f;
}
global.Lerp = global.Lerp || Lerp;
let LerpP = function(a, b, f){
  if (f <= 0) return a;
  if (f >= 1) return b;
  return {
    'x': Lerp(a.x, b.x, f),
    'y': Lerp(a.y, b.y, f)
  };
}
global.LerpP = global.LerpP || LerpP;

let cv = {
  status_norm: 0,
  status_auto: 1,
  status_pause: 2,

  touch_begin: 0,
  touch_move: 1,
  touch_ended: 2,

  scaleMin: 0.7,
};

export default class DrawLayout extends Component {
  constructor(props){
    super(props);
    this._panResponder = {};
    this.drawWord = null;
    this.nowPos = 0;
    this.touchLastPoint = null;

    this.mousePosition = null;
    this.lastMousePostion = null;
    this.listUsedPoint = [];
    
    this.status = cv.status_norm;
    for(var i=0;i<data.character.length;i++){
      var points = data.character[i].points;
      for(var k=0;k<points.length;k++){
        points[k].x = points[k].x * scaleWidth;
        points[k].y = points[k].y * scaleWidth;
      }
    }
    this.wrongCount = 0;
    this.state={
      blnUpdate: false,
    };
  }
  setUpdate(){
    this.setState({
      blnUpdate: !this.state.blnUpdate,
    });
  }
  componentWillMount() {
    this._panResponder = PanResponder.create({
      onStartShouldSetPanResponder: this.onStartShouldSetPanResponder.bind(this),
      onMoveShouldSetPanResponder: this.onMoveShouldSetPanResponder.bind(this),
      onPanResponderGrant: this.onPanResponderGrant.bind(this),
      onPanResponderMove: this.onPanResponderMove.bind(this),
      onPanResponderRelease: this.onPanResponderRelease.bind(this),
      onPanResponderTerminate: this.onPanResponderTerminate.bind(this),
    });
  }
  componentDidMount() {
    this._autoUpdate = setInterval(this.autoUpdate.bind(this), 1/60);
  }
  componentWillUnmount() {
    this._autoUpdate && clearInterval(this._autoUpdate);
  }
  
  onStartShouldSetPanResponder(e, g){
    if (this.status == cv.status_auto || this.status == cv.status_pause){
      return false;
    }
    return true;
  }
  onMoveShouldSetPanResponder(e, g){
    if (this.status == cv.status_auto || this.status == cv.status_pause){
      return false;
    }
    return true;
  }
  onPanResponderGrant(e, g){
    if (g.numberActiveTouches == 1){
      this.mousePosition = {
        x: e.nativeEvent.locationX,
        y: e.nativeEvent.locationY
      };
      this.ResetDrawPoint();
      this.AddUsePoint(this.mousePosition);
    }
  }
  onPanResponderMove(e, g){
    if (g.numberActiveTouches == 1){
      this.mousePosition = {
        x: e.nativeEvent.locationX,
        y: e.nativeEvent.locationY
      };
      this.AddUsePoint(this.mousePosition);
    }
  }
  onPanResponderRelease(e, g){
    this.endPanResponder(e, g);
  }
  onPanResponderTerminate(e, g){
    this.endPanResponder(e, g);
  }
  endPanResponder(e, g){
    if (this.blnCanDraw){
      this.mousePosition = {
        x: e.nativeEvent.locationX,
        y: e.nativeEvent.locationY
      };
      this.AddUsePoint(this.mousePosition);
    }
  }
  ResetDrawPoint(){
    this.listUsedPoint = [];
  }
  AddUsePoint(pos, scale){
    var d = new Path();
    d.moveTo(pos.x, pos.y);
    d.arc(0, 4, 4).arc(0, -4, 4).close();
    this.listUsedPoint.push(
      <Shape d={d} fill={'blue'}/>
    );
    this.setUpdate();
  }
  SetEndPoint(bln){

  }
  autoUpdate(){
    
  }
  render() {
    return (
      <View style={styles.container} {...this._panResponder.panHandlers}>
        <DrawWord style={styles.upView} ref={(r)=>{this.drawWord = r}} data={data}/>
        <View style={styles.mouseView}>
          <Surface ref={'lineView'} width={ScreenWidth} height={ScreenHeight}>
            {this.listUsedPoint}
          </Surface>
        </View>
      </View>
    );
  }
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    backgroundColor: '#F5FCFF'
  },
  mouseView:{
    position: 'absolute',
    left: 0,
    top: 0,
    width: ScreenWidth,
    height: ScreenHeight,
    backgroundColor: 'transparent'
  },
  upView:{
    width: curWidth,
    height: curWidth,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#CCC'
  },
  downView: {
    width: ScreenWidth,
    flexDirection: 'row',
    justifyContent: 'space-around',
    marginTop: minUnit * 5,
  },
  autoWriteBtn:{
    marginBottom: minUnit * 5,
    marginLeft: minUnit * 5,
  },
  autoWriteText:{
    fontSize: minUnit * 6,
    textAlign: 'center',
  },
  restartBtn:{
    marginBottom: minUnit * 5,
    marginRight: minUnit * 5,
  },
  restartText:{
    fontSize: minUnit * 6,
    textAlign: 'center',
  }
});
