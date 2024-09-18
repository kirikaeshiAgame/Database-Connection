using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace labajetire
{
    public partial class Form1 : Form
    {
        private string connectionString = "Server=127.0.0.1;Port=5432;User Id=товой Пользователь;Password= тут я думаю понятно;Database=твоя база данных";
        private NpgsqlDataAdapter dataAdapter;
        private DataTable dataTable;
        private bool isAdding = false; // Флаг для отслеживания процесса добавления
        private string studentIdToDelete;


        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            button1.Click += button1_Click;
            button2.Click += button2_Click; // Добавляем обработчик события для второй кнопки
            this.button3.Click += new System.EventHandler(this.button3_Click);
            textBox4.TextChanged += textBox4_TextChanged;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // Создаем DataTable для хранения данных из базы данных
            dataTable = new DataTable();
            dataGridView1.DataSource = dataTable;

            // Загрузка данных в выпадающий список групп
            LoadGroups();
        }

        private void LoadGroups()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Создаем команду для выборки данных из таблицы "ГРУППЫ"
                    string sql = "SELECT id, name FROM public.group";
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sql, connection))
                    {
                        DataTable groupsTable = new DataTable();
                        adapter.Fill(groupsTable);

                        // Заполняем выпадающий список групп данными из базы данных
                        comboBox1.DataSource = groupsTable;
                        comboBox1.DisplayMember = "name";
                        comboBox1.ValueMember = "id";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных о группах: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Создаем команду для выборки данных из таблицы "Студенты"
                    string sql = "SELECT * FROM public.student";
                    dataAdapter = new NpgsqlDataAdapter(sql, connection);

                    // Заполняем DataTable данными из базы данных
                    dataTable.Clear(); // Очищаем таблицу перед заполнением новыми данными
                    dataAdapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных из базы данных: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isAdding) // Если процесс добавления уже запущен, выходим
                return;

            isAdding = true; // Устанавливаем флаг добавления в true

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверяем наличие студента с такими же данными
                    string checkStudentExistsQuery = "SELECT COUNT(*) FROM public.student WHERE name = @name AND surname = @surname AND id_group = @groupId";
                    using (NpgsqlCommand checkCmd = new NpgsqlCommand(checkStudentExistsQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@name", textBox1.Text);
                        checkCmd.Parameters.AddWithValue("@surname", textBox2.Text);
                        checkCmd.Parameters.AddWithValue("@groupId", comboBox1.SelectedValue);

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (count > 0)
                        {
                            ///MessageBox.Show("Студент с такими данными уже существует!");
                            return; // Прерываем операцию добавления
                        }
                    }

                    // Создаем SQL-запрос для вставки нового студента в таблицу
                    string sql = "INSERT INTO public.student (name, surname, middle_name, id_group) VALUES (@name, @surname, @middleName, @groupId)";

                    // Создаем команду с параметрами
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    {
                        // Присваиваем значения параметрам
                        cmd.Parameters.AddWithValue("@name", textBox1.Text);
                        cmd.Parameters.AddWithValue("@surname", textBox2.Text);
                        cmd.Parameters.AddWithValue("@middleName", textBox3.Text);
                        cmd.Parameters.AddWithValue("@groupId", comboBox1.SelectedValue); // Получаем выбранный идентификатор группы из выпадающего списка

                        // Выполняем команду
                        cmd.ExecuteNonQuery();
                    }
                }

                // После успешной вставки обновляем данные в DataGridView
                UpdateDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении данных в базу данных: " + ex.Message);
            }
            finally
            {
                isAdding = false; // Устанавливаем флаг добавления в false после окончания операции
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Выполняем удаление студента по номеру, введенному в textBox4
            DeleteStudentById(textBox4.Text);
        }
        private void DeleteStudentById(string studentId)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Удаляем зависимые записи из таблицы progress
                    string deleteProgressSql = "DELETE FROM public.progress WHERE id_student = @studentId";
                    using (NpgsqlCommand deleteProgressCmd = new NpgsqlCommand(deleteProgressSql, connection))
                    {
                        deleteProgressCmd.Parameters.AddWithValue("@studentId", Convert.ToInt64(studentId));
                        deleteProgressCmd.ExecuteNonQuery();
                    }

                    // Удаляем зависимые записи из таблицы type_of_occupation
                    string deleteOccupationSql = "DELETE FROM public.type_of_occupation WHERE id_teacher = @studentId";
                    using (NpgsqlCommand deleteOccupationCmd = new NpgsqlCommand(deleteOccupationSql, connection))
                    {
                        deleteOccupationCmd.Parameters.AddWithValue("@studentId", Convert.ToInt64(studentId));
                        deleteOccupationCmd.ExecuteNonQuery();
                    }

                    // Теперь удаляем самого студента из таблицы public.student
                    string deleteStudentSql = "DELETE FROM public.student WHERE id = @studentId";
                    using (NpgsqlCommand deleteStudentCmd = new NpgsqlCommand(deleteStudentSql, connection))
                    {
                        deleteStudentCmd.Parameters.AddWithValue("@studentId", Convert.ToInt64(studentId));
                        deleteStudentCmd.ExecuteNonQuery();
                    }

                    // После успешного удаления обновляем данные в DataGridView
                    UpdateDataGridView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении студента из базы данных: " + ex.Message);
            }
        }

        private void UpdateDataGridView()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Создаем команду для выборки данных из таблицы "Студенты"
                    string sql = "SELECT * FROM public.student";
                    dataAdapter = new NpgsqlDataAdapter(sql, connection);

                    // Очищаем DataTable перед заполнением новыми данными
                    dataTable.Clear();

                    // Заполняем DataTable данными из базы данных
                    dataAdapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных в DataGridView: " + ex.Message);
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            // При изменении текста в textBox4 обновляем переменную studentIdToDelete
            studentIdToDelete = textBox4.Text;
        }

         
        //private bool sortByAscending = true; // Флаг для отслеживания направления сортировки

        private bool filterAscending = true;
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Создаем команду для выборки данных из таблицы "Студенты" с фильтрацией
                    // по идентификатору (id) сначала от минимального до максимального и обратно
                    string sortOrder = filterAscending ? "ASC" : "DESC";
                    string sql = "SELECT * FROM public.student ORDER BY id " + sortOrder;
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                    {
                        // Создаем адаптер данных и заполняем DataTable данными из базы данных
                        dataAdapter = new NpgsqlDataAdapter(cmd);
                        dataTable.Clear(); // Очищаем таблицу перед заполнением новыми данными
                        dataAdapter.Fill(dataTable);
                    }

                    // Инвертируем направление фильтрации для следующего нажатия кнопки
                    filterAscending = !filterAscending;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных из базы данных: " + ex.Message);
            }
        }
    }
}