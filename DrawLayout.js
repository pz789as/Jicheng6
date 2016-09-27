/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 * @flow
 */

import React, { Component } from 'react';
import {
  StyleSheet,
  Text,
  View,
  PanResponder,
  TouchableOpacity,
  StatusBar,
} from 'react-native';

import Utils from './Utils';
import DrawWord from './DrawWord';

var data = require('./data/瞅.json');

let cv = {
  status_norm: 0,
  status_auto: 1,
  status_pause: 2,
};

export default class DrawLayout extends Component {
  constructor(props){
    super(props);
    this._panResponder = {};
    this.drawWord = null;
    this.nowPos = 0;
    this.touchLastPoint = null;
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
      if (this.drawWord){
        var tp = {
          x: e.nativeEvent.locationX,
          y: e.nativeEvent.locationY
        };
        this.touchLastPoint = tp;
        var idx = this.drawWord.drawIdx;
        if (idx >= 0){
          var points = data.character[idx].orgPoints;
          var dis = Utils.DisP(tp, data.character[idx].orgPoints[Math.min(this.nowPos, points.length-1)]);
          if (dis < unitDisSt){
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
            var tempD = Utils.DisP(tp, this.touchLastPoint);
            if (tempD >= 1){
              var count = Math.max(1, parseInt(tempD / 4));
              var oldPos = this.nowPos;
              for(var i=0;i<count;i++){
                var sp = Utils.LerpP(this.touchLastPoint, tp, (i+1) / count);
                if (i==tempD-1){
                  sp = tp;
                }
                var dis = Utils.DisP(tp, points[Math.min(this.nowPos, points.length-1)]);
                if (dis < unitDisMv){
                  this.nowPos++;
                }
              }
              if (this.nowPos != oldPos){
                this.drawWord.DrawingPecent(this.nowPos / points.length);
              }
            }
            this.touchLastPoint = tp;
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
            this.setDrawNext();
          }else{
            console.log('书写完毕!');
          }
        }else if (this.nowPos == 0){
          this.wrongCount ++;
          if (this.wrongCount == 3){
            this.drawWord.setStrokeBlink();
            this.wrongCount = 0;
          }
        }
      }
    }
  }
  setDrawNext(){
    this.drawWord.setEndDraw();
    this.drawWord.drawIdx++;
    this.nowPos = 0;
    this.drawWord.setBeginDraw();
    this.wrongCount = 0;
  }
  restartWrite(){
    if (this.status == cv.status_auto || this.status == cv.status_pause){
      this.autoWriteStop();
    }
    if (this.drawWord){
      this.nowPos = 0;
      this.drawWord.setRestart();
      this.wrongCount = 0;
    }
  }
  autoWriteStop(){
    this.status = cv.status_norm;
    this.setUpdate();
  }
  autoWrite(){
    if (this.status == cv.status_norm){
      this.wrongCount = 0;
      this.status = cv.status_auto;
      this.nowPos = 0;
      if (this.drawWord){
        this.drawWord.setRestart();
      }
      this.setUpdate();
    }else if (this.status == cv.status_auto){
      this.status = cv.status_pause;
      this.setUpdate();
    }else if (this.status == cv.status_pause){
      this.status = cv.status_auto;
      this.setUpdate();
    }
  }
  autoUpdate(){
    if (this.status == cv.status_auto){
      if (this.drawWord){
        var idx = this.drawWord.drawIdx;
        if (idx>=0){
          var points = data.character[idx].orgPoints;
          if (this.nowPos >= points.length + 10){
            if (this.drawWord.drawIdx < data.character.length - 1){
              this.setDrawNext();
            }else{
              this.status = cv.status_norm;
              this.setUpdate();
            }
          }else{
            this.nowPos += 0.25;
            this.drawWord.DrawingPecent(this.nowPos / points.length);
          }
        }
      }
    }
  }
  getAutoText(){
    if (this.status == cv.status_auto){
      return '暂停书写';
    }else if (this.status == cv.status_norm){
      return '自动书写';
    }else if (this.status == cv.status_pause){
      return '继续书写';
    }
  }
  render() {
    return (
      <View style={styles.container} {...this._panResponder.panHandlers}>
        <DrawWord style={styles.upView} ref={(r)=>{this.drawWord = r}} data={data}/>
        <View style={styles.downView}>
          <TouchableOpacity style={styles.autoWriteBtn} onPress={this.autoWrite.bind(this)}>
            <Text style={styles.autoWriteText}>
              {this.getAutoText()}
            </Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.restartBtn} onPress={this.restartWrite.bind(this)}>
            <Text style={styles.restartText}>
              重新开始
            </Text>
          </TouchableOpacity>
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
