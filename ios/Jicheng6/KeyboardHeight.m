//
//  KeyboardHeight.m
//  Jicheng6
//
//  Created by guojicheng on 16/11/7.
//  Copyright © 2016年 Facebook. All rights reserved.
//

#import "KeyboardHeight.h"

@implementation KeyboardHeight

RCT_EXPORT_MODULE();

- (instancetype)init
{
  self = [super init];
  if (self) {
    self.kbHeight = 0;
//    [[NSNotificationCenter defaultCenter] addObserver:self
//                                             selector:@selector(keyboardWillShow:)
//                                                 name:UIKeyboardWillShowNotification
//                                               object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(keyboardDidShow:)
                                                 name:UIKeyboardDidShowNotification
                                               object:nil];
//    [[NSNotificationCenter defaultCenter] addObserver:self
//                                             selector:@selector(keyboardWillChangeFrame:)
//                                                 name:UIKeyboardWillChangeFrameNotification
//                                               object:nil];
  }
  return self;
}

-(void)keyboardWillShow:(NSNotification*) aNotification
{
  //获取键盘的高度
//  NSDictionary *userInfo = [aNotification userInfo];
//  NSValue *aValue = [userInfo objectForKey:UIKeyboardFrameEndUserInfoKey];
//  CGRect keyboardRect = [aValue CGRectValue];
//  if (_kbHeight != keyboardRect.size.height){
//    _kbHeight = keyboardRect.size.height;
//    [self heightChanged:_kbHeight];
//  }
  CGFloat curkeyboardHeight = [[[aNotification userInfo] objectForKey:@"UIKeyboardBoundsUserInfoKey"] CGRectValue].size.height;
  CGRect begin = [[[aNotification userInfo] objectForKey:@"UIKeyboardFrameBeginUserInfoKey"] CGRectValue];
  CGRect end = [[[aNotification userInfo] objectForKey:@"UIKeyboardFrameEndUserInfoKey"] CGRectValue];
  if (begin.size.height > 0 && begin.origin.y - end.origin.y > 0){
    if (_kbHeight != curkeyboardHeight){
      _kbHeight = curkeyboardHeight;
      [self heightChanged:_kbHeight];
    }
  }
}

-(void)keyboardDidShow:(NSNotification*) aNotification
{
  //获取键盘的高度
  NSDictionary *userInfo = [aNotification userInfo];
  NSValue *aValue = [userInfo objectForKey:UIKeyboardFrameEndUserInfoKey];
  CGRect keyboardRect = [aValue CGRectValue];
  NSLog(@"did show: %f", keyboardRect.size.height);
  if (_kbHeight != keyboardRect.size.height){
    _kbHeight = keyboardRect.size.height;
    [self heightChanged:_kbHeight];
  }
}

-(void)keyboardWillChangeFrame:(NSNotification*) aNotification
{
  //获取键盘的高度
  NSDictionary *userInfo = [aNotification userInfo];
  NSValue *aValue = [userInfo objectForKey:UIKeyboardFrameEndUserInfoKey];
  CGRect keyboardRect = [aValue CGRectValue];
  NSLog(@"will change frame: %f", keyboardRect.size.height);
  if (_kbHeight != keyboardRect.size.height){
//    _kbHeight = keyboardRect.size.height;
//    [self heightChanged:_kbHeight];
  }
}

RCT_REMAP_METHOD(getKBHeight,
                 resolver:(RCTPromiseResolveBlock)resolve
                 rejecter:(RCTPromiseRejectBlock)reject)
{
  resolve([[NSNumber alloc]initWithInt:_kbHeight]);
}

- (NSArray<NSString *> *)supportedEvents
{
  return @[@"heightChanged"];
}

-(void)heightChanged:(int)height
{
  [self sendEventWithName:@"heightChanged" body:[NSNumber numberWithUnsignedInt:height]];
}

@end
