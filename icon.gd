extends AnimatedSprite2D

@onready var transparent_window = get_node("/root/Main/TransparentWindow")

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	frame = 0


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_menu_button_mouse_entered() -> void:
	frame = 1

func _on_menu_button_mouse_exited() -> void:
	frame = 0
