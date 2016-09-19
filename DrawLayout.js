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
} from 'react-native';

import Dimensions from 'Dimensions';
let ScreenWidth = Dimensions.get('window').width;
global.ScreenWidth = global.ScreenWidth || ScreenWidth;
let ScreenHeight = Dimensions.get('window').height;
global.ScreenHeight = global.ScreenHeight || ScreenHeight;
let curWidth = Math.min(ScreenWidth, ScreenHeight);
let scaleWidth = curWidth / 400;

import DrawWord from './DrawWord';

var data = require('./data/瞅.json');

let Dis = function(x, y){
  return Math.sqrt(x * x + y * y);
};
global.Dis = global.Dis || Dis;
let DisP = function(p1, p2){
  return Dis(p1.x - p2.x, p1.y - p2.y);
};
global.DisP = global.DisP || DisP;

export default class DrawLayout extends Component {
  constructor(props){
    super(props);
    this._panResponder = {};
    this.drawWord = null;
    this.nowPos = 0;
    for(var i=0;i<data.character.length;i++){
      var points = data.character[i].points;
      for(var k=0;k<points.length;k++){
        points[k].x = points[k].x * scaleWidth;
        points[k].y = points[k].y * scaleWidth;
      }
    }
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
    
  }
  onStartShouldSetPanResponder(e, g){
    return true;
  }
  onMoveShouldSetPanResponder(e, g){
    return true;
  }
  onPanResponderGrant(e, g){
    if (g.numberActiveTouches == 1){
      if (this.drawWord){
        var tp = {
          x: e.nativeEvent.locationX,
          y: e.nativeEvent.locationY
        };
        var idx = this.drawWord.drawIdx;
        if (idx >= 0){
          var points = data.character[idx].orgPoints;
          var dis = DisP(tp, data.character[idx].orgPoints[Math.min(this.nowPos, points.length-1)]);
          if (dis < 10){
            this.nowPos++;
            this.drawWord.DrawingPecent(this.nowPos / points.length);
          }
        }
      }
    }
  }
  onPanResponderMove(e, g){
    if (g.numberActiveTouches == 1){
      if (this.drawWord){
        var tp = {
          x: e.nativeEvent.locationX,
          y: e.nativeEvent.locationY
        };
        var idx = this.drawWord.drawIdx; 
        if (idx >= 0){
          var points = data.character[idx].orgPoints;
          if (this.nowPos - 1 < points.length){
            var dis = DisP(tp, points[Math.min(this.nowPos, points.length-1)]);
            if (dis < 10){
              this.nowPos++;
              this.drawWord.DrawingPecent(this.nowPos / points.length);
            }
          }
        }
      }
    }
  }
  onPanResponderRelease(e, g){
    this.endPanResponder(e, g);
  }
  onPanResponderTerminate(e, g){
    this.endPanResponder(e, g);
  }
  endPanResponder(e, g){
    if (this.drawWord){
      var idx = this.drawWord.drawIdx; 
      if (idx >= 0){
        var points = data.character[idx].orgPoints;
        if (this.nowPos >= points.length){
          if (this.drawWord.drawIdx < data.character.length - 1){
            console.log('学习下一笔');
            this.drawWord.setEndDraw();
            this.drawWord.drawIdx++;
            this.nowPos = 0;
            this.drawWord.setBeginDraw();
          }else{
            console.log('书写完毕!');
          }
        }
      }
    }
  }
  restartWrite(){
    if (this.drawWord){
      this.nowPos = 0;
      this.drawWord.setRestart();
    }
  }
  render() {
    return (
      <View style={styles.container} {...this._panResponder.panHandlers}>
        <DrawWord ref={(r)=>{this.drawWord = r}} data={data}/>
        <TouchableOpacity style={styles.restart} onPress={this.restartWrite.bind(this)}>
          <Text style={styles.restartText}>
            重新开始
          </Text>
        </TouchableOpacity>
      </View>
    );
  }
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#F5FCFF'
  },
  restart:{
    position: 'absolute',
    left: ScreenWidth/2,
    bottom: 40,
  },
  restartText:{
    fontSize: 30,
    textAlign: 'center',
  },
  welcome: {
    fontSize: 20,
    textAlign: 'center',
    margin: 10,
  },
  instructions: {
    textAlign: 'center',
    color: '#333333',
    marginBottom: 5,
  },
});
