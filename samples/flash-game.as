package {
    import flash.display.Sprite;
    import flash.events.MouseEvent;
    import flash.events.Event;
    
    public class SimpleGame extends Sprite {
        private var player:Sprite;
        private var score:int = 0;
        
        public function SimpleGame() {
            init();
        }
        
        private function init():void {
            player = new Sprite();
            player.graphics.beginFill(0xFF0000);
            player.graphics.drawCircle(0, 0, 20);
            player.graphics.endFill();
            player.x = stage.stageWidth / 2;
            player.y = stage.stageHeight / 2;
            addChild(player);
            
            stage.addEventListener(MouseEvent.CLICK, onStageClick);
            addEventListener(Event.ENTER_FRAME, onEnterFrame);
        }
        
        private function onStageClick(e:MouseEvent):void {
            score += 10;
            trace("Score: " + score);
        }
        
        private function onEnterFrame(e:Event):void {
            player.x += 2;
            if (player.x > stage.stageWidth) {
                player.x = 0;
            }
        }
    }
}
