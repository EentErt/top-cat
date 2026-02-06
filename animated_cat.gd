extends AnimatedSprite2D

var flip = false

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass

func walk():
	play("walk")
	if !flip_h:
		$Head.offset = Vector2(0, -1)
	else:
		$Head.offset = Vector2(15, -1)
	
func sit():
	flip_h = false
	play("sit")
	$Head.offset = Vector2(7, -1)
	
func loaf():
	flip_h = false
	play("loaf")
	$Head.offset = Vector2(7, 13)
	
func nap():
	flip_h = false
	play("nap")
	$Head.offset = Vector2(7, 13)
	$Head.frame = 6
