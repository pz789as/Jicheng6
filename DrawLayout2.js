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

import Utils from './Utils';
import DrawWord from './DrawWord2';
import DrawTouch from './DrawTouch2';

let cv = {
  status_norm: 0,
  status_auto: 1,
  status_pause: 2,

  touch_begin: 0,
  touch_move: 1,
  touch_ended: 2,
};

let data = [
  // 'M 283 231 L 282 261 L 281 302 L 279 351 L 275 400 L 270 444 L 270 444 L 262 482 L 253 519 L 241 555 L 229 589 L 215 623 L 215 623 L 198 658 L 178 693 L 158 727 L 140 756 L 128 776',
  // 'M 298 255 L 361 239 L 453 214 L 555 189 L 646 176 L 705 183 L 705 183 L 724 219 L 716 277 L 694 345 L 672 414 L 663 471 L 663 471 L 667 517 L 675 560 L 687 598 L 703 634 L 723 667 L 723 667 L 749 701 L 783 735 L 818 765 L 851 785 L 875 789 L 875 789 L 890 771 L 899 733 L 904 688 L 907 645 L 910 617',
  // 'M 555 314 L 545 343 L 531 384 L 515 431 L 498 477 L 480 516 L 480 516 L 462 546 L 442 573 L 422 597 L 402 618 L 383 638 L 383 638 L 364 656 L 345 673 L 327 688 L 311 699 L 300 708',
  // 'M 353 390 L 392 436 L 449 503 L 512 578 L 569 645 L 608 691',
  'M 418 65 L 434 76 L 458 92 L 485 110 L 509 126 L 525 137',
  'M 255 209 L 325 200 L 428 188 L 542 174 L 645 162 L 715 153',
  'M 353 211 L 346 226 L 333 248 L 323 272 L 322 293 L 340 303 L 340 303 L 386 301 L 456 289 L 533 273 L 603 258 L 650 248',
  'M 353 346 L 359 365 L 367 392 L 376 422 L 384 449 L 390 468',
  'M 365 362 L 403 354 L 459 340 L 521 327 L 577 317 L 613 316 L 613 316 L 625 327 L 622 346 L 611 369 L 597 391 L 590 405',
  'M 390 440 L 423 435 L 471 428 L 524 419 L 572 412 L 605 407',
  'M 178 508 L 178 528 L 178 556 L 178 588 L 178 621 L 175 649 L 175 649 L 171 673 L 167 695 L 161 716 L 153 735 L 143 754 L 143 754 L 128 773 L 109 791 L 88 808 L 70 822 L 58 832',
  'M 180 521 L 198 510 L 225 492 L 255 475 L 282 470 L 300 486 L 300 486 L 310 536 L 314 613 L 314 698 L 310 775 L 303 824 L 303 824 L 290 839 L 269 833 L 247 814 L 227 794 L 213 782',
  'M 193 614 L 202 612 L 216 610 L 232 607 L 246 605 L 255 603',
  'M 178 686 L 190 684 L 209 682 L 229 679 L 248 677 L 260 675',
  'M 385 501 L 383 530 L 380 573 L 378 621 L 375 664 L 373 693',
  'M 400 508 L 422 501 L 455 490 L 491 480 L 523 477 L 545 486 L 545 486 L 554 515 L 555 559 L 551 608 L 546 654 L 543 684',
  'M 465 545 L 464 563 L 463 589 L 461 618 L 459 648 L 455 673 L 455 673 L 451 694 L 447 713 L 441 731 L 434 748 L 423 765 L 423 765 L 407 782 L 386 799 L 363 814 L 344 828 L 330 837',
  'M 488 704 L 495 711 L 506 720 L 517 731 L 528 740 L 535 747',
  'M 638 475 L 638 497 L 638 529 L 638 565 L 636 602 L 633 634 L 633 634 L 628 661 L 623 687 L 616 711 L 607 734 L 595 756 L 595 756 L 579 777 L 558 798 L 536 817 L 516 833 L 503 845',
  'M 650 495 L 670 486 L 699 471 L 732 457 L 760 449 L 778 453 L 778 453 L 781 473 L 775 505 L 765 543 L 755 583 L 750 617 L 750 617 L 751 648 L 755 678 L 761 707 L 769 733 L 778 756 L 778 756 L 790 775 L 804 790 L 820 803 L 835 813 L 850 821 L 850 821 L 864 828 L 879 832 L 893 835 L 905 833 L 913 826 L 913 826 L 917 811 L 918 788 L 916 763 L 914 740 L 913 725',
  'M 655 601 L 663 611 L 676 626 L 689 642 L 702 657 L 710 667',
];
let redio = curWidth / 1000;
let perdio = redio * 700;

export default class DrawLayout2 extends Component {
  constructor(props){
    super(props);
    this._panResponder = {};
    this._autoUpdate = null;
    this.status = cv.status_norm;
    this.mousePosition = {x: 0, y: 0};
    this.wordData = [];
    this.strokeIndex = 0;
    this.strokeTime = 0;
    this.state={
      blnUpdate: false,
    };
    this.initWord();
  }
  initWord()
  {
    for(var i=0;i<data.length;i++){
      var d = data[i].split(' ');
      var points = [];
      var len = 0;
      var runTime = 0;
      var p = null;
      for(var j=0;j<d.length;){
        var np = {
          x: parseInt(d[j + 1]) * redio,
          y: parseInt(d[j + 2]) * redio
        };
        if (p != null){
          len += Utils.DisP(p, np);
        }
        p = np;
        points.push(p);
        j+=3;
      }
      runTime = len / perdio;
      this.wordData.push({
        'points': points,
        'len': len,
        'runTime': runTime,
        'show': false,
      });
    }
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
    this._autoUpdate = setInterval(this.autoUpdate.bind(this), 1.0/60);
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
    }
  }
  onPanResponderMove(e, g){
    if (g.numberActiveTouches == 1){
      this.mousePosition = {
        x: e.nativeEvent.locationX,
        y: e.nativeEvent.locationY
      };
    }
  }
  onPanResponderRelease(e, g){
    this.endPanResponder(e, g);
  }
  onPanResponderTerminate(e, g){
    this.endPanResponder(e, g);
  }
  endPanResponder(e, g){
    this.mousePosition = {
      x: e.nativeEvent.locationX,
      y: e.nativeEvent.locationY
    };
  }
  autoUpdate(){
    if (this.strokeIndex < this.wordData.length){
      this.strokeTime++;
      if (this.strokeTime / 60 >= this.wordData[this.strokeIndex].runTime){
        this.wordData[this.strokeIndex].show = true;
        this.strokeIndex++;
        this.strokeTime = 0;
      }
      this.setUpdate();
    }
  }
  render() {
    var backLine = [];
    var showLine = [];
    var strokeWidth = 5 / redio;
    for(var i=0;i<this.wordData.length;i++){
      var perT = this.strokeTime / 60 / this.wordData[i].runTime;
      var perD = perT * this.wordData[i].len;
      var p = new Path();
      var phalf = new Path();
      var point = null, lpoint = null;
      for(var j=0;j<this.wordData[i].points.length;j++){
        point = {
          x: this.wordData[i].points[j].x, 
          y: this.wordData[i].points[j].y
        };
        if (j==0){
          p.moveTo(point.x, point.y);
          if (i == this.strokeIndex){
            phalf.moveTo(point.x, point.y);
          }
        }else {
          p.lineTo(point.x, point.y);
          if (i == this.strokeIndex && perD > 0){
            var dis = Utils.DisP(lpoint, point);
            if (perD > dis){
              phalf.lineTo(point.x, point.y);
            }else {
              var halfp = Utils.LerpP(lpoint, point, perD / dis);
              phalf.lineTo(halfp.x, halfp.y);
            }
            perD -= dis;
          }
        }
        lpoint = point;
      }
      backLine.push(
        <Shape key={i} d={p} stroke={'#bbb'} strokeWidth={strokeWidth}/>
      );
      if (i == this.strokeIndex){
        showLine.push(
          <Shape key={i} d={phalf} stroke={'#222'} strokeWidth={strokeWidth}/>
        );
      }else if (this.wordData[i].show) {
        showLine.push(
          <Shape key={i} d={p} stroke={'#222'} strokeWidth={strokeWidth}/>
        );
      }
    }
    return (
      <View style={styles.container} {...this._panResponder.panHandlers}>
        <Surface ref={'lineView'} width={curWidth} height={curWidth} style={{left: 0, backgroundColor: 'grey'}}>
          {backLine}
          {showLine}
        </Surface>
      </View>
    );
  }
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    width: ScreenWidth,
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
    position: 'absolute',
    left: relativeX,
    top: relativeY,
    width: curWidth,
    height: curWidth,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#CCC'
  },
  downChange:{
    position: 'absolute',
    left: 0,
    bottom: 10,
    width: ScreenWidth,
    height: minUnit * 10,
    flexDirection: 'row',
    justifyContent: 'space-around',
  },
  downView: {
    position: 'absolute',
    left: 0,
    bottom: 10 + minUnit * 10,
    width: ScreenWidth,
    height: minUnit * 10,
    flexDirection: 'row',
    justifyContent: 'space-around',
  },
  buttonStyle:{
    
  },
  buttonTextStyle:{
    fontSize: minUnit * 6,
    textAlign: 'center',
  },
  tipsStyle:{
    position: 'absolute',
    left: ScreenWidth*3/8,
    top: ScreenHeight/2-minUnit*3,
    width: ScreenWidth/4,
    height: minUnit*6,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: 'rgb(0,0,0)',
    opacity: 0.5
  },
  tipsText:{
    color: 'red',
    fontSize: minUnit*4,
    textAlign: 'center',
  },
});
